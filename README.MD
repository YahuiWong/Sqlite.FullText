# Sqlite.FullText

> Sqlite full-text index help library

Usage example

```C#
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
```


reference

https://www.cnblogs.com/percyboy/