#region Licenced under the BSD licence
/*
  Copyright (c) 2011, 破宝（percyboy） http://percyboy.cnblogs.com/
  All rights reserved.

  Redistribution and use in source and binary forms, with or without modification, 
  are permitted provided that the following conditions are met:

  * Redistributions of source code must retain the above copyright notice, 
    this list of conditions and the following disclaimer.
  * Redistributions in binary form must reproduce the above copyright notice, 
    this list of conditions and the following disclaimer in the documentation 
    and/or other materials provided with the distribution.
  * Neither the name of the 破宝（percyboy） nor the names of its contributors 
    may be used to endorse or promote products derived from this software 
    without specific prior written permission.

  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
  AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
  ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
  FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
  (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
  LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
  AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
  OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF 
  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;

namespace Sqlite.FullText
{
    //注意：
    // SQLiteFtsTokenizer 只有在 PrepareToStart 方法时才能取得要切分的字符串。
    // Lucene 的 Tokenizer 支持实例化后、通过 Reset 方法指定要切分的字符串；
    // 而 TokenFilter 不支持，必须在实例化时才能指定。
    //
    // 第一个例子用事件方式解决这一矛盾，支持 TokenStream（包括 TokenFilter 和 Tokenizer）。
    // 第二个例子不用事件，只支持 Tokenizer，使用相对方便些。
    // 可以根据需要调整。

    //使用示例：
    //using (SQLiteConnection connection = new SQLiteConnection("Data Source=filename"))
    //{
    //    LuceneTokenStreamAdapter adapter = new LuceneTokenStreamAdapter();
    //    adapter.TokenStart = delegate 
    //    {
    //        Tokenizer tokenizer = new Lucene.Net.Analysis.Standard.StandardTokenizer(new StringReader(adapter.InputString));
    //        adapter.SetTokenStream(tokenizer); 
    //    };
    //    connection.Open();
    //    adapter.RegisterMe(connection);
    //
    //    ....操作SQLite....
    //
    //    connection.Close();
    //}
    public class LuceneTokenStreamAdapter : SQLiteFtsTokenizer
    {
        private TokenStream ts;
        private CharTermAttribute termAttr;
        private OffsetAttribute offsetAttr;

        public LuceneTokenStreamAdapter()
        {
        }

        public event EventHandler TokenStart;

        protected virtual void OnTokenStart(EventArgs args)
        {
            if (TokenStart != null)
            {
                TokenStart(this, args);
            }
        }

        public void SetTokenStream(TokenStream ts)
        {
            this.ts = ts;
            if (this.ts.HasAttribute<IOffsetAttribute>())
                this.offsetAttr = (OffsetAttribute)this.ts.GetAttribute<IOffsetAttribute>();
            if (this.ts.HasAttribute<ICharTermAttribute>())
                this.termAttr =(CharTermAttribute) this.ts.GetAttribute<ICharTermAttribute>();

        }

        protected override void PrepareToStart()
        {
            OnTokenStart(EventArgs.Empty);

            if (this.ts == null)
            {
                throw new ApplicationException("Please call SetTokenStream while TokenStart event fires.");
            }
        }

        protected override bool MoveNext()
        {
            if (this.ts.IncrementToken())
            {
                this.Token = this.termAttr.ReflectAsString(true);
                this.TokenIndexOfString = this.offsetAttr.StartOffset;
                this.NextIndexOfString = this.offsetAttr.EndOffset;
                return true;
            }
            return false;
        }
    }


    //使用示例：
    //using (SQLiteConnection connection = new SQLiteConnection("Data Source=filename"))
    //{
    //    Tokenizer tokenizer = new Lucene.Net.Analysis.Standard.StandardTokenizer();
    //    LuceneTokenizerAdapter adapter = new LuceneTokenizerAdapter(tokenizer);
    //    connection.Open();
    //    adapter.RegisterMe(connection);
    //
    //    ....操作SQLite....
    //
    //    connection.Close();
    //}
    public class LuceneTokenizerAdapter : SQLiteFtsTokenizer
    {
        private Tokenizer tokenizer;
        private CharTermAttribute termAttr;
        private OffsetAttribute offsetAttr;

        public LuceneTokenizerAdapter(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;

            this.termAttr = (CharTermAttribute)this.tokenizer.GetAttribute<ICharTermAttribute>();
            this.offsetAttr = (OffsetAttribute)this.tokenizer.GetAttribute<IOffsetAttribute>();
        }

        protected override void PrepareToStart()
        {
            tokenizer.Reset();
            tokenizer.SetReader(new StringReader(this.InputString));
        }

        protected override bool MoveNext()
        {
            if (this.tokenizer.IncrementToken())
            {
                this.Token = this.termAttr.ReflectAsString(true);
                this.TokenIndexOfString = this.offsetAttr.StartOffset;
                this.NextIndexOfString = this.offsetAttr.EndOffset;
                return true;
            }
            return false;
        }
    }

}
