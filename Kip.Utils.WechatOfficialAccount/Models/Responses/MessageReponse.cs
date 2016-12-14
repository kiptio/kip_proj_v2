using Kip.Utils.WechatOfficialAccount.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Kip.Utils.WechatOfficialAccount.Models.Responses
{
    public abstract class MsgResponseBase
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public double CreateTime { get; set; }
        public string MsgType { get; set; }

        public MsgResponseBase()
        {
            this.CreateTime = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds;
        }

        public virtual void ConvertFromRequest(TextRequestModel request)
        {
            this.ToUserName = request.FromUserName;
            this.FromUserName = request.ToUserName;
            this.CreateTime = request.CreateTime;
        }
    }

    [XmlRoot("xml")]
    public class TextResponseModel : MsgResponseBase
    {
        public TextResponseModel()
            : base()
        {
            this.MsgType = "text";
        }

        public string Content { get; set; }
    }

    [XmlRoot("xml")]
    public class NewsResponseModel : MsgResponseBase
    {
        public NewsResponseModel()
            : base()
        {
            this.MsgType = "news";
            this.Articles = new List<Item>();
        }

        [Serializable]
        public struct Item
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string PicUrl { get; set; }
            public string Url { get; set; }
        }

        public int ArticleCount
        {
            set { }
            get { return this.Articles.Count(); }
        }

        [XmlArrayItem("item", Type = typeof(Item))]
        public List<Item> Articles { get; set; }
    }
}
