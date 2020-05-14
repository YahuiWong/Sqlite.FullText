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
            string text = "SQLiteFtsTokenizer ֻ���� PrepareToStart ����ʱ����ȡ��Ҫ�зֵ��ַ�����";
            string text2 = "��ʱ��������Ҫͬʱִ��һЩ������Ȼ�����Щ�����Ľ�����л��ܣ��Դﵽ���첽�����Ͳ�����ʱ��Ч������ʱ���ǻῼ��ʹ��Task����Task.WhenAll���������ó������������������������õ�Person���SayHello������ʱ�����ͻᱨ����Ȼ������ڶ�����������Ϊfalseʱ���ڵ��øó�Ա��ʱ�򲻻ᱨ�����ǻᱨ�����档΢��sqlite����ȫ��������������ô�����أ���׿ƽ̨��֧�����ĵ�tokenizer��";
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
            string text = "SQLiteFtsTokenizer ֻ���� PrepareToStart ����ʱ����ȡ��Ҫ�зֵ��ַ�����";
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
            string text = "SQLiteFtsTokenizer ֻ���� PrepareToStart ����ʱ����ȡ��Ҫ�зֵ��ַ�����";
            var result = tokenizer.TestMe(text);
            Assert.IsNotNull(result);
        }
        [Test]
        public void SqliteTest()
        {

            //ע�⣺
            // SQLiteFtsTokenizer ֻ���� PrepareToStart ����ʱ����ȡ��Ҫ�зֵ��ַ�����
            // Lucene �� Tokenizer ֧��ʵ������ͨ�� Reset ����ָ��Ҫ�зֵ��ַ�����
            // �� TokenFilter ��֧�֣�������ʵ����ʱ����ָ����
            //
            // ��һ���������¼���ʽ�����һì�ܣ�֧�� TokenStream������ TokenFilter �� Tokenizer����
            // �ڶ������Ӳ����¼���ֻ֧�� Tokenizer��ʹ����Է���Щ��
            // ���Ը�����Ҫ������

            //ʹ��ʾ����
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=test.db"))
            {
                CJKTokenizer tokenizer = new CJKTokenizer();
                connection.Open();
                tokenizer.RegisterMe(connection); //ע���д���

                //����
                SQLiteCommand cmd = new SQLiteCommand(connection);
                cmd.CommandText = "CREATE VIRTUAL TABLE docs USING fts3(title, content, tokenize=cjk)";
                cmd.ExecuteNonQuery();

                //��������
                cmd.CommandText = "INSERT INTO docs (title, content) VALUES (?, ?)";
                SQLiteParameter p1 = new SQLiteParameter();
                p1.DbType = System.Data.DbType.String;
                p1.Value = "���Ա���";
                cmd.Parameters.Add(p1);
                SQLiteParameter p2 = new SQLiteParameter();
                p2.DbType = System.Data.DbType.String;
                p2.Value = "��������";
                cmd.Parameters.Add(p2);
                cmd.ExecuteNonQuery();

                //����
                cmd.CommandText = "SELECT docid, title, content FROM docs WHERE docs MATCH '����'";
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