using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.WechatOfficialAccount.Models.Exceptions
{
    public class WechatApiException : Exception
    {
        public WechatApiException(int code, string content)
        {
            Code = code;
            Content = content;
        }

        public int Code { get; set; }

        public string Content { get; set; }
    }
}
