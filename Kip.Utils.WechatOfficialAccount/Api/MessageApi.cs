using Kip.Utils.Core;
using Kip.Utils.WechatOfficialAccount.Models.Requests;
using Kip.Utils.WechatOfficialAccount.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kip.Utils.WechatOfficialAccount.Api
{
    public partial class WechatApi
    {
        public void ResponseMessage(HttpRequestBase request, HttpResponseBase response, Action<TextRequestModel, string> action)
        {
            TextRequestModel requestModel = XmlUtils.XmlDeserialize<TextRequestModel>(request.InputStream);

            string responseContent = "";
            action(requestModel, responseContent);

            response.ContentType = "text/xml";
            response.Write(responseContent);
        }

        // 自动回复 - 文本信息
        public string ResponseTextMessage(TextRequestModel requestModel)
        {
            TextResponseModel responseModel = new TextResponseModel();
            responseModel.ConvertFromRequest(requestModel);
            responseModel.Content = requestModel.Content;
            return responseModel.SerializeObjectToXml();
        }

        // 自动回复 - 图文信息
        public string ResponseNewsMessage(TextRequestModel requestModel)
        {
            NewsResponseModel responseModel = new NewsResponseModel();
            responseModel.ConvertFromRequest(requestModel);
            responseModel.Articles.Add(new NewsResponseModel.Item
            {
                Title = "test",
                Description = "test_Description",
                PicUrl = "www.baidu.com",
                Url = "www.baidu.com",
            });
            return responseModel.SerializeObjectToXml();
        }

    }
}
