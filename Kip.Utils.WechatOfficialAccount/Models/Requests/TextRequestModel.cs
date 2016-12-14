using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.WechatOfficialAccount.Models.Requests
{
    public class TextRequestModel
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public double CreateTime { get; set; }
        public string MsgType { get; set; }
        public string Content { get; set; }
    }
}
