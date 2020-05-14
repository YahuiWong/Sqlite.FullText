using jieba.NET;
using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using NUnit.Framework;
using System.Data.SQLite;
using System.IO;

namespace Sqlite.FullText.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }
        [Test]
        public void JieBaLuceneTokenStreamAdapterTest()
        {
            string text = "SQLiteFtsTokenizer 只有在 PrepareToStart 方法时才能取得要切分的字符串。";
            string text2 = "有时候我们需要同时执行一些操作，然后把这些操作的结果进行汇总，以达到用异步处理降低操作耗时的效果，此时我们会考虑使用Task，而Task.WhenAll则排上了用场。这样当我们在主程序中用到Person类的SayHello方法的时候程序就会报错。当然，如果第二个参数设置为false时，在调用该成员的时候不会报错，但是会报出警告。微信sqlite本地全文索引搜索是怎么做的呢？安卓平台不支持中文的tokenizer？";
            LuceneTokenStreamAdapter adapter = new LuceneTokenStreamAdapter();
            adapter.TokenStart += delegate
            {
                
                Tokenizer tokenizer = new JieBaTokenizer(new StringReader(adapter.InputString), TokenizerMode.Search);
                tokenizer.Reset();
                adapter.SetTokenStream(tokenizer);
            };

            var result = adapter.TestMe(text);
            var result2 = adapter.TestMe(text2);
            Assert.Pass();
        }

        [Test]
        public void JieBaLuceneTokenizerAdapterTest()
        {
            string text = "SQLiteFtsTokenizer 只有在 PrepareToStart 方法时才能取得要切分的字符串。";
            var reader = text;
            Tokenizer tokenizer = new Lucene.Net.Analysis.Standard.StandardTokenizer(Lucene.Net.Util.LuceneVersion.LUCENE_48, new StringReader(" "));
            LuceneTokenizerAdapter adapter = new LuceneTokenizerAdapter(tokenizer);
            var r = adapter.TestMe(text);
            Assert.Pass();
        }
        [Test]
        public void CJKTokenizerTest()
        {
            CJKTokenizer tokenizer = new CJKTokenizer();
            string text = "SQLiteFtsTokenizer 只有在 PrepareToStart 方法时才能取得要切分的字符串。";
            var result = tokenizer.TestMe(text);
            Assert.IsNotNull(result);
        }
        [Test]
        public void SqliteTest()
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
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=test.db"))
            {
                CJKTokenizer tokenizer = new CJKTokenizer();
                connection.Open();
                tokenizer.RegisterMe(connection); //注册切词器

                //建表
                SQLiteCommand cmd = new SQLiteCommand(connection);
                cmd.CommandText = "CREATE VIRTUAL TABLE docs USING fts3(title, content, tokenize=cjk)";
                cmd.ExecuteNonQuery();

                //插入数据
                cmd.CommandText = "INSERT INTO docs (title, content) VALUES (?, ?)";
                SQLiteParameter p1 = new SQLiteParameter();
                p1.DbType = System.Data.DbType.String;
                p1.Value = "测试标题";
                cmd.Parameters.Add(p1);
                SQLiteParameter p2 = new SQLiteParameter();
                p2.DbType = System.Data.DbType.String;
                p2.Value = "测试内容";
                cmd.Parameters.Add(p2);
                cmd.ExecuteNonQuery();

                //检索
                cmd.CommandText = "SELECT docid, title, content FROM docs WHERE docs MATCH '测试'";
                SQLiteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {

                }
                dr.Close();

                connection.Close();
            }
        }
    }
}