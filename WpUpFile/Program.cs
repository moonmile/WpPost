using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpPost;

namespace WpUpFile
{
    class Program
    {
        /// <summary>
        /// Wordpress へ画像ファイルをアップロードする
        /// - wpupfile -s 640 640 $file1
        /// - 画像サイズを指定してサムネールを作る
        /// - 元ファイル\nサムネール で返す
        ///   20161024_03org.jpg
        ///   20161024_03thum.jpg
        /// </summary>
        /// <param name="args"></param>
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

            int width = 640;
            int height = 640;
            string file = "";
            if (args.Count() == 1)
            {
                file = args[0];
            }
            else if ( args.Count() == 4 )
            {
                if (args[0].StartsWith("-s") || args[0].StartsWith("/s"))
                {
                    width = int.Parse(args[1]);
                    height = int.Parse(args[2]);
                    file = args[3];
                }
                else
                {
                    return;
                }
            }

            // ファイル名を作る
            var file_org = file.Replace(".jpg", "org.jpg");
            var file_thum = file.Replace(".jpg", "thum.jpg");

            var bmp = Bitmap.FromFile(file);

            int w2 = bmp.Width;
            int h2 = bmp.Height;

            if ( bmp.Width > width )
            {
                w2 = width;
                h2 = height * bmp.Height / bmp.Width;
            }
            else if ( bmp.Height > height )
            {
                h2 = height;
                w2 = width * bmp.Width / bmp.Height;
            }
            var bmp2 = new Bitmap(bmp, new Size(w2, h2));
            var g = Graphics.FromImage(bmp2);
            g.DrawImage(bmp, new Rectangle(0, 0, w2, h2));



            // オリジナルをjpgで保存
            bmp.Save(file_org, System.Drawing.Imaging.ImageFormat.Jpeg);
            // サムネールをjpgで保存
            bmp2.Save(file_thum, System.Drawing.Imaging.ImageFormat.Jpeg);


            var post_org = new ImageFile() { Filename = file_org };
            var ret_org = tools.NewImage(post_org) ;
            var post_thum = new ImageFile() { Filename = file_thum } ;
            var ret_thum = tools.NewImage(post_thum);

            Console.WriteLine( ret_org.url);
            Console.WriteLine( ret_thum.url);
        }
    }
}
