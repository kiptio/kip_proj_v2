using Kip.Utils.WechatOfficialAccount.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kip.Utils.WechatOfficialAccount.Test
{
    class TestMain
    {
        #region [验证]
        public void Valid(HttpRequestBase request, HttpResponseBase response)
        {
            WechatApi.TestInstance.Valid(request, response);
        }
        #endregion

        #region [获取AccessToken]
        public string GetAccessToken()
        {
            return WechatApi.TestInstance.GetAccessToken();
        }
        #endregion

        #region [自动回复]
        public void ResponseMessage(HttpRequestBase request, HttpResponseBase response)
        {
            WechatApi.TestInstance.ResponseMessage(request, response, (requestModel, responseContent) =>
            {
                if (requestModel.Content == "news")
                {
                    responseContent = WechatApi.TestInstance.ResponseNewsMessage(requestModel);
                }
                else
                {
                    responseContent = WechatApi.TestInstance.ResponseTextMessage(requestModel);
                }
            });
        }
        #endregion
    }
}
