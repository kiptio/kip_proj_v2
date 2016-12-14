using Kip.Utils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kip.Utils.WechatOfficialAccount.Api
{
    public partial class WechatApi
    {
        // 开发者ID
        private string appId { get; set; }
        private string appSecret { get; set; }
        // token，与微信公共平台上的Token保持一致
        private string token { get; set; }

        public WechatApi(string appId, string appSecret, string token)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this.token = token;
        }

        #region [测试实例]
        private static WechatApi testInstance;
        public static WechatApi TestInstance
        {
            get
            {
                if (null == testInstance) testInstance = new WechatApi("wx9dcb2427df27d22c", "e5c4700e5f9e619c475fc2f28120b4e4", "weixin_test");
                return testInstance;
            }
        }
        #endregion

        #region [正式实例]
        private static WechatApi proInstance;
        public static WechatApi ProInstance
        {
            get
            {
                if (null == proInstance) proInstance = new WechatApi("wx09b720dc808e00be", "78c2b6b18ad2bfb247ac7bf6aaa3d0cb", "weixin_test");
                return proInstance;
            }
        }
        #endregion

        #region [验证签名，检验是否是从微信服务器上发出的请求]
        /// <summary>
        /// 验证签名，检验是否是从微信服务器上发出的请求
        /// </summary>
        /// <param name="request">Http请求</param>
        /// <returns>是否验证通过</returns>
        public void Valid(HttpRequestBase request, HttpResponseBase response)
        {
            string signature = request.QueryString["signature"].ToString();
            string timestamp = request.QueryString["timestamp"].ToString();
            string nonce = request.QueryString["nonce"].ToString();
            string echoStr = request.QueryString["echoStr"].ToString();

            if (!string.IsNullOrEmpty(echoStr) && CheckSignature(timestamp, nonce, signature))
            {
                //将随机生成的 echostr 参数 原样输出
                response.Write(echoStr);
            }
        }

        /// <summary>
        /// 验证签名，检验是否是从微信服务器上发出的请求
        /// </summary>
        /// <param name="model">请求参数模型 Model</param>
        /// <returns>是否验证通过</returns>
        public bool CheckSignature(string timestamp, string nonce, string signature)
        {
            //创建数组，将 token, timestamp, nonce 三个参数加入数组
            string[] array = { token, timestamp, nonce };
            //进行排序
            Array.Sort(array);
            //拼接为一个字符串
            string tempStr = String.Join("", array);
            //对字符串进行 SHA1加密
            tempStr = SecurityUtils.GetSHA1HashString(tempStr).ToLower();
            //判断signature 是否正确
            if (tempStr.Equals(signature))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region [获取AccessToken]
        public string GetAccessToken()
        {
            string cacheKey = "weixin_accessToken_" + this.appId;
            string accessToken = CacheUtils.Get<string>(cacheKey);
            if (null == accessToken)
            {
                string urlformat = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";

                var result = HttpUtils.HttpGet(string.Format(urlformat, appId, appSecret));
                var response = JsonUtils.Deserialize<Models.Responses.AccessTokenResponse>(result);

                if (response.IsError)
                {
                    throw new Models.Exceptions.WechatApiException(response.ErrorCode, response.ErrorMessage);
                }
                accessToken = response.Access_Token;

                CacheUtils.Add<string>(cacheKey, accessToken);
            }

            return accessToken;
        }
        #endregion

    }
}
