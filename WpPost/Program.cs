using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace WpPost
{
    class Program
    {
        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.main(args);
        }

        /// <summary>
        /// メイン関数
        /// </summary>
        /// <param name="args"></param>
        public void main(string[] args)
        {
            PostTools tools = new PostTools();
            MySetting setting = new MySetting();

            // 設定読み込み
            setting = setting.Load("wppost.config");
            tools.Username = setting.Username;
            tools.Password = setting.Password;
            tools.Url = setting.Url;

            // カテゴリ一覧を表示
            if (args.Count() == 1)
            {
                if (args[0].StartsWith("-c") ||
                     args[0].StartsWith("/c"))
                {
                    List<CategoryPair> lst = tools.GetCategoryList();
                    var lst2 = from t in lst orderby t.categoryId select t;

                    foreach (var cat in lst2)
                    {
                        Console.Out.WriteLine("ID:{0} name:{1} desc:{2}",
                            cat.categoryId,
                            cat.categoryName,
                            cat.description);
                    }
                    return;
                }
            }
            if (args.Count() == 2)
            {
                if (args[0].StartsWith("-g") ||
                     args[0].StartsWith("/g"))
                {
                    Post pst = tools.GetPost(args[1]);
                    TextWriter twr = Console.Out;
                    WritePostFile(twr, pst);
                }
                return;
            }

            // 投稿ファイル読み込み
            TextReader tr;
            if (args.Count() == 0)
            {
                tr = Console.In;
            }
            else
            {
                tr = new StreamReader(args[0], Encoding.GetEncoding("shift_jis"));
            }
            Post post = ReadPostFile(tr);
            tr.Close();

            post.Publish = false; // 下書きで投稿
            string id = tools.NewPost(post);
            Console.WriteLine("id: {0}", id);

            if (id == "") return;

            TextWriter tw;
            if (tr == Console.In)
            {
                tw = Console.Out;
            } else {
                tw = new StreamWriter(args[0] ,false, Encoding.GetEncoding("shift_jis"));
            }
            WritePostFile(tw, post);
            tw.Close();
        }

        /// <summary>
        /// ファイル読み込み
        /// </summary>
        /// <param name="sr">テキストストリーム</param>
        /// <returns></returns>
        public Post ReadPostFile(TextReader sr)
        {
            string line = "";
            Post post = new Post();
            post.PostDate = DateTime.Now;
            post.Publish = true;

            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                {
                    break;
                }
                if (line.StartsWith("Title:"))
                {
                    post.Title = line.Replace("Title:", "").Trim();
                }
                else if (line.StartsWith("ID:"))
                {
                    post.ID = line.Replace("ID:", "").Trim();
                }
                else if (line.StartsWith("CategoryID:"))
                {
                    post.CategoryID = line.Replace("CategoryID:", "").Trim();
                }
                else if (line.ToLower().StartsWith("page:"))
                {
                    post.CustomFileds.Add("page", line.Replace("Page:", "").Trim());
                }
                else if (line.ToLower().StartsWith("section:"))
                {
                    post.CustomFileds.Add("section", line.Replace("Section:", "").Trim());
                }
            }
            if (line == "")
            {
                while ((line = sr.ReadLine()) != null)
                {
                    post.Content += line + "\n";
                }
            }
            if (post.Content == "")
            {
                // フォーマットエラー
                return null;
            }
            return post;
        }
        /// <summary>
        /// ファイル書き出し
        /// </summary>
        /// <param name="sr">テキストストリーム</param>
        /// <param name="post">記事</param>
        /// <returns></returns>
        public void WritePostFile(TextWriter sr, Post post)
        {
            sr.WriteLine("ID: " + post.ID);
            sr.WriteLine("Title: " + post.Title);
            sr.WriteLine("Date: " + post.PostDate);
            sr.WriteLine("CategoryID: " + post.CategoryID);
            foreach (var f in post.CustomFileds)
            {
                sr.WriteLine("{0}: {1}", 
                    f.Key.Substring(0,1).ToUpper() + f.Key.Substring(1),
                    f.Value);
            }
            sr.WriteLine("");
            sr.WriteLine(post.Content);
        }
    }

    public class MySetting
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }

        // コンストラクタ
        public MySetting()
        {
            Username = "";
            Password = "";
            Url = "";
        }

        // 設定を保存
        public void Save(string filename)
        {
            XmlSerializer xs = new XmlSerializer(this.GetType());
            FileStream fs = new FileStream(filename, FileMode.Create);
            xs.Serialize(fs, this);
        }
        // 設定を読み込み
        public MySetting Load(string filename)
        {
            XmlSerializer xs = new XmlSerializer(this.GetType());
            try
            {
                FileStream fs = new FileStream(filename, FileMode.Open);
                MySetting me = (MySetting)xs.Deserialize(fs);
                return me;
            }
            catch
            {
                // 最初の場合は初期値
                return new MySetting();
            }
        }
    }
}
