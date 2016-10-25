using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CookComputing.XmlRpc;
using System.IO;

namespace WpPost
{
    /// <summary>
    /// 記事投稿クラス
    /// </summary>
    public class Post
    {
        // 記事ID
        public string ID { get; set; }
        // 投稿日時
        public DateTime PostDate { get; set; }
        // タイトル
        public string Title { get; set; }
        // 本文(HTML形式)
        public string Content { get; set; }
        // カテゴリID
        public string CategoryID { get; set; }
        // カスタムフィールド
        public Dictionary<string, string> CustomFileds { get; set; }
        // 公開
        public bool Publish { get; set; }

        public Post()
        {
            ID = "";
            PostDate = DateTime.Now;
            CategoryID = "";
            CustomFileds = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 画像ファイルアップロードクラス
    /// </summary>
    public class ImageFile
    {
        public string Filename { get; set; }

    }
    public class UpFile
    {
        public string name { get; set; }
        public string type { get; set; }
        public byte[] bits { get; set; }
        public bool overwrite { get; set; }

    }

    public class PostTools
    {
        // ユーザ名
        public string Username { get; set; }
        // パスワード
        public string Password { get; set; }
        // 登録先のURL
        public string Url { get; set; }


        /// <summary>
        /// 新しい記事を投稿する
        /// </summary>
        /// <param name="post">投稿記事</param>
        /// <returns>投稿した記事のID</returns>
        public string NewPost( Post post ) 
        {
            //プロキシクラスのインスタンスを作成
            IBlogger proxy = (IBlogger)
                CookComputing.XmlRpc.XmlRpcProxyGen.Create(
                typeof(IBlogger));

            //URLを指定
            proxy.Url = this.Url;

            string id = "";
            // content を生成
            Content cont = new Content();
            cont.title = post.Title;
            cont.description = post.Content;
            if (post.PostDate == null )
            {
                post.PostDate = DateTime.Now;
            }
            cont.dateCreated = post.PostDate;
            cont.custom_fields = null;
            if (post.CustomFileds != null)
            {
                cont.custom_fields = new Fields[post.CustomFileds.Count ];
                int i = 0;
                foreach (var f in post.CustomFileds)
                {
                    cont.custom_fields[i] = new Fields();
                    cont.custom_fields[i].key = f.Key;
                    cont.custom_fields[i].value = f.Value;
                    i++;
                }
            }
            else
            {
                cont.custom_fields = new Fields[0];
            }
            try
            {
                if (post.ID == "")
                {
                    //blogger.getRecentPostsを呼び出す
                    id = proxy.newPost(
                        "1",            // 念のため1にしておく
                        this.Username,
                        this.Password,
                        cont,
                        post.Publish);
                }
                else
                {
                    // 編集時
                    if (proxy.editPost(
                            post.ID,
                            this.Username,
                            this.Password,
                            cont,
                            post.Publish))
                    {
                        // 成功
                        id = post.ID;
                    }
                    else
                    {
                        id = "";
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("エラー：" + ex.Message);
                return "" ;
            }

            if (post.CategoryID != "")
            {
                Category[] cat = new Category[1];
                cat[0] = new Category();
                cat[0].categoryId = post.CategoryID;
                cat[0].isPrimary = true;

                // カテゴリのエラーは無視する
                bool ret = proxy.setPostCategories(
                    id,
                    this.Username,
                    this.Password,
                    cat);
            }

            post.ID = id;
            return id;
        }

        public List<CategoryPair> GetCategoryList()
        {
            //プロキシクラスのインスタンスを作成
            IBlogger proxy = (IBlogger)
                CookComputing.XmlRpc.XmlRpcProxyGen.Create(
                typeof(IBlogger));

            //URLを指定
            proxy.Url = this.Url;
            List<CategoryPair> lst = new List<CategoryPair>();
            CategoryPair[] ret = proxy.getCategoryList("1", this.Username, this.Password);

            foreach (var f in ret)
            {
                lst.Add(f);
            }
            return lst;
        }

        /// <summary>
        /// 既存の記事を取得する
        /// </summary>
        /// <param name="post">投稿記事</param>
        /// <returns>投稿した記事のID</returns>
        public Post GetPost(string postid)
        {
            //プロキシクラスのインスタンスを作成
            IBlogger proxy = (IBlogger)
                CookComputing.XmlRpc.XmlRpcProxyGen.Create(
                typeof(IBlogger));

            //URLを指定
            proxy.Url = this.Url;

            RetPost retpost = proxy.getPost(
                postid,
                this.Username,
                this.Password);
            if (retpost == null)
            {
                return null;
            }

            Post post = new Post();

            post.ID = retpost.postid.ToString();
            post.Title = retpost.title;
            post.PostDate = retpost.dateCreated;
            post.Content = retpost.description;
            if (retpost.categories != null &&
                 retpost.categories.Count() > 0)
            {
                post.CategoryID = retpost.categories[0];
            }
            if (retpost.custom_fields != null)
            {
                post.CustomFileds = new Dictionary<string, string>();
                foreach (var f in retpost.custom_fields)
                {
                    post.CustomFileds.Add(f.key, f.value);
                }
            }

            return post;
        }

        /// <summary>
        /// 新しい画像を投稿する
        /// </summary>
        /// <param name="post">投稿記事</param>
        /// <returns>投稿した記事のID</returns>
        public RetUpFile NewImage(ImageFile post)
        {
            //プロキシクラスのインスタンスを作成
            IBlogger proxy = (IBlogger)
                CookComputing.XmlRpc.XmlRpcProxyGen.Create(
                typeof(IBlogger));

            //URLを指定
            proxy.Url = this.Url;

            // ファイル名を小文字に変換
            post.Filename = post.Filename.ToLower();

            // UpFile を生成
            UpFile upfile = new UpFile();
            upfile.name = post.Filename;
            upfile.type = "image/jpeg";
            upfile.bits = null;
            upfile.overwrite = true;

            // ファイルを読み込む
            long len = new FileInfo(post.Filename).Length;
            byte[] data = new byte[len];
            BinaryReader rd = new BinaryReader(
                File.OpenRead(post.Filename));
            rd.Read(data, 0, (int)len);
            rd.Close();

            // BASE64に変換しない
            // upfile.bits = Convert.ToBase64String(data);
            upfile.bits = data;
            // ファイルタイプを変更
            if (upfile.name.EndsWith(".jpg") ||
                 upfile.name.EndsWith(".jpeg"))
            {
                upfile.type = "image/jpeg";
            }
            else if (upfile.name.EndsWith(".png"))
            {
                upfile.type = "image/png";
            }
            else if (upfile.name.EndsWith(".gif"))
            {
                upfile.type = "image/gif";
            }

            RetUpFile ret;
            try
            {
                //blogger.getRecentPostsを呼び出す
                ret = proxy.uploadFile(
                    "1",            // 念のため1にしておく
                    this.Username,
                    this.Password,
                    upfile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("エラー：" + ex.Message);
                return null;
            }

            return ret;
        }
    }


    public interface IBlogger : IXmlRpcProxy
    {
        /// <summary>
        /// 新規投稿
        /// </summary>
        /// <param name="blogid">無視</param>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="content">本文</param>
        /// <param name="publish">公開するかどうか</param>
        /// <returns>エントリのIDを返す</returns>
        [XmlRpcMethod("metaWeblog.newPost")]
        string newPost(
            string blogid,
            string username,
            string password,
            Content content,
            bool publish);

        /// <summary>
        /// 記事を編集
        /// </summary>
        /// <param name="postid">記事ID</param>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="content">本文</param>
        /// <param name="publish">公開するかどうか</param>
        /// <returns>エントリのIDを返す</returns>
        [XmlRpcMethod("metaWeblog.editPost")]
        bool editPost(
            string postid,
            string username,
            string password,
            Content content,
            bool publish);

        /// <summary>
        /// <summary>
        /// 記事を取得
        /// </summary>
        /// <param name="postid">記事ID</param>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns>投稿記事の内容を返す</returns>
        [XmlRpcMethod("metaWeblog.getPost")]
        RetPost getPost(
            string postid,
            string username,
            string password);

        /// 指定記事にカテゴリを設定
        /// </summary>
        /// <param name="postid">記事ID</param>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <param name="categories">カテゴリ</param>
        /// <returns></returns>
        [XmlRpcMethod("mt.setPostCategories")]
        bool setPostCategories(
            string postid,
            string username,
            string password,
            Category[] categories);

        [XmlRpcMethod("wp.getCategories")]
        CategoryPair[] getCategoryList(
            string blogid,
            string username,
            string password);

        [XmlRpcMethod("wp.uploadFile")]
        RetUpFile uploadFile(
            string postid,
            string username,
            string password,
            UpFile upfile);



    }
    /// <summary>
    /// カスタムフィールドの要素クラス
    /// </summary>
    public class Fields
    {
        public string key;
        public string value;
    }
    /// <summary>
    /// コンテンツクラス
    /// </summary>
    public class Content
    {
        public string title;
        public string description;
        public DateTime dateCreated;
        public Fields[] custom_fields;
    }
    // カテゴリの要素クラス
    public class Category
    {
        public string categoryId;
        public bool isPrimary;
    }
    // カテゴリ取得時のクラス
    public class CategoryPair
    {
        public string categoryId;
        public string parentId;
        public string description; 
        public string categoryName; 
        public string htmlUrl ;
        public string rssUrl;  
    }
    // 投稿記事の取得クラス
    public class RetPost
    {
        public string   userid;
        public DateTime dateCreated;
        public int      postid;
    	public string   description;
    	public string   title;
	    public string   link;
    	public string   permaLink;
        public string[] categories;
        public Fields[] custom_fields;
    }

    // ファイルアップロードの戻り値クラス
    public class RetUpFile
    {
        public string id;
        public string file;
        public string url;
        public string type;
    }
}
