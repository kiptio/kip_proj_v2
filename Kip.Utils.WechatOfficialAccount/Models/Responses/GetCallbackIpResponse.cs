using Kip.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.WechatOfficialAccount.Models.Responses
{
    public class GetCallbackIpResponse : ApiResponseBase
    {
        [NewtonJsonProperty("ip_list")]
        public string[] IpList { get; set; }
    }
}
