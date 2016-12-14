using Kip.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.WechatOfficialAccount.Models.Responses
{
    public abstract class ApiResponseBase
    {
        public bool IsError
        {
            get { return ErrorCode != 0; }
        }

        [NewtonJsonProperty("errcode")]
        public int ErrorCode { get; set; }

        [NewtonJsonProperty("errmsg")]
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            if (IsError) return String.Format("errcode:{0}, errmsg:{1}", ErrorCode, ErrorMessage);

            return base.ToString();
        }
    }
}
