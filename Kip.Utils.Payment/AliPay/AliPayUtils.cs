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

        #region [支付宝-App支付结果异步通知]
        /// <summary>
        /// https://doc.open.alipay.com/docs/doc.htm?spm=a219a.7629140.0.0.LWyWoI&treeId=204&articleId=105301&docType=1#s6
        /// </summary>
        /// <returns></returns>
        public string Alipay_Notify(Dictionary<string, string> paramsMap)
        {
            var alipayUtils = new AliPayUtils();
            if (alipayUtils.CheckSign(paramsMap))
            {
                /// <summary>
                /// 1、商户需要验证该通知数据中的out_trade_no是否为商户系统中创建的订单号，
                /// 2、判断total_amount是否确实为该订单的实际金额（即商户订单创建时的金额），
                /// 3、校验通知中的seller_id（或者seller_email) 是否为out_trade_no这笔单据的对应的操作方（有的时候，一个商户可能有多个seller_id/seller_email），
                /// 4、验证app_id是否为该商户本身。
                /// 上述1、2、3、4有任何一个验证不通过，则表明本次通知是异常通知，务必忽略。
                /// 在上述验证通过后商户必须根据支付宝不同类型的业务通知，正确的进行不同的业务处理，并且过滤重复的通知结果数据。在支付宝的业务通知中，只有交易通知状态为TRADE_SUCCESS或TRADE_FINISHED时，支付宝才会认定为买家付款成功。
                /// </summary>
                //订单号Id
                int orderId = int.Parse(paramsMap["out_trade_no"]);
                decimal totalAmount = decimal.Parse(paramsMap["total_amount"]);
                string sellerId = paramsMap["seller_id"];
                string appId = paramsMap["app_id"];
                string tradeStatus = paramsMap["trade_status"];
                string tradeNo = paramsMap["trade_no"];

                //var order = orderService.GetById(orderId);
                //if (null != order && order.TotalAmount == totalAmount && AliPayConfig.seller_id == sellerId && AliPayConfig.app_id == appId)
                //{
                //    // do success
                //    return "success";
                //}

                return "success";
            }
            else
            {
                // do failure
            }

            return "failure";
        }
        #endregion

        private static IAopClient GetAlipayClient()
        {
            return new DefaultAopClient(AliPayConfig.server_url, AliPayConfig.app_id, AliPayConfig.merchant_private_key, true);
        }

    }
}
