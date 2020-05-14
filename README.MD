# Sqlite.FullText

> Sqlite full-text index help library

Usage example

```C#
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
```


reference

https://www.cnblogs.com/percyboy/