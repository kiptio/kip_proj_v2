using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kip.Utils.Payment.AliPay
{
    public class AliPayUtils
    {
        public string Sign(IDictionary<string, string> paramaters)
        {
            return AlipaySignature.RSASign(paramaters, AliPayConfig.merchant_private_key, "utf-8", true, "RSA");
        }

        #region [生成预订单信息]
        public string GeneratePaymentInfo(PaymentOrder order)
        {
            // 业务请求参数
            var biz_content = new
            {
                subject = order.ProductName,
                out_trade_no = order.OrderNo,
                total_amount = order.TotalAmount,
                product_code = "QUICK_MSECURITY_PAY",
            };

            SortedDictionary<string, string> sortedData = new SortedDictionary<string, string>();
            sortedData.Add("app_id", AliPayConfig.app_id);
            sortedData.Add("method", "alipay.trade.app.pay");
            sortedData.Add("format", "json");
            sortedData.Add("charset", "utf-8");
            sortedData.Add("sign_type", "RSA");
            sortedData.Add("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); //发送请求的时间，格式"yyyy-MM-dd HH:mm:ss"
            sortedData.Add("version", "1.0");
            sortedData.Add("notify_url", AliPayConfig.notify_url);
            sortedData.Add("biz_content", PaymentTools.Serializer(biz_content));  //业务请求参数的集合

            //商户请求参数的签名串
            string sign = AlipaySignature.RSASign(sortedData, AliPayConfig.merchant_private_key, "utf-8", true, "RSA");
            sortedData.Add("sign", sign);

            string formData = "";
            foreach (var item in sortedData)
            {
                formData += string.Format("&{0}={1}", item.Key, HttpUtility.UrlEncode(item.Value, Encoding.UTF8));
            }
            return formData.Substring(1);
        }
        #endregion

        #region [查询交易状态]
        public string QueryTradeStatus(PaymentOrder order)
        {
            IAopClient client = GetAlipayClient();
            AlipayTradeQueryRequest request = new AlipayTradeQueryRequest();
            request.BizContent = "{" +
            "    \"out_trade_no\":\"" + order.OrderNo + "\"," +
            "    \"trade_no\":\"" + order.TradeNo + "\"" +
            "  }";
            AlipayTradeQueryResponse response = client.Execute(request);
            if (!response.IsError)
            {
                if (order.OrderNo == response.OutTradeNo && order.TotalAmount == decimal.Parse(response.TotalAmount))
                {
                    return response.TradeStatus;
                }
            }
            return "TRADE_NOTFOUND";
        }
        #endregion

        #region [退款]
        public string Refund(PaymentOrder order)
        {
            IAopClient client = GetAlipayClient();
            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
            request.BizContent = "{" +
            "    \"out_trade_no\":\"" + order.OrderNo + "\"," +
            "    \"trade_no\":\"" + order.TradeNo + "\"," +
            "    \"refund_amount\":\"" + order.TotalAmount + "\"," +
            "    \"refund_reason\":\"正常退款\"" +
            "  }";

            AlipayTradeRefundResponse response = client.Execute(request);
            return string.Format("code[{0}],msg[{1}],subcode[{2}],submsg[{3}]",
                response.Code, response.Msg, response.SubCode, response.SubMsg);
        }
        #endregion

        #region [退款查询]
        public string QueryTradeRefundStatus(PaymentOrder order)
        {
            IAopClient client = GetAlipayClient();
            AlipayTradeFastpayRefundQueryRequest request = new AlipayTradeFastpayRefundQueryRequest();
            request.BizContent = "{" +
            "    \"out_trade_no\":\"" + order.OrderNo + "\"," +
            "    \"trade_no\":\"" + order.TradeNo + "\"," +
            "    \"out_request_no\":\"" + order.TradeNo + "\"," +
            "  }";

            AlipayTradeFastpayRefundQueryResponse response = client.Execute(request);
            if (!response.IsError)
            {
                if (order.OrderNo == response.OutTradeNo && order.TotalAmount == decimal.Parse(response.TotalAmount))
                {
                    return response.SubMsg;
                }
            }
            return "TRADE_NOTFOUND";
        }
        #endregion

        #region [签名验证]
        public bool CheckSign(IDictionary<string, string> paramsMap)
        {
            // V1 will remove "sign" and "sign_type"
            return AlipaySignature.RSACheckV1(paramsMap, AliPayConfig.alipay_merchant_public_key, "utf-8");
        }
        #endregion

        private static IAopClient GetAlipayClient()
        {
            return new DefaultAopClient(AliPayConfig.server_url, AliPayConfig.app_id, AliPayConfig.merchant_private_key, true);
        }

    }
}
