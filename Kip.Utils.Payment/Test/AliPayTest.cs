using Kip.Utils.Payment.AliPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kip.Utils.Payment.Test
{
    class AliPayTest
    {
        #region [生成订单预付信息]
        public string GeneratePaymentInfo()
        {
            PaymentOrder paymentOrder = new PaymentOrder();
            paymentOrder.OrderNo = "1000000858585"; // "your order no"
            paymentOrder.ProductName = "your product name";
            paymentOrder.Price = 200;               // "your product price"
            paymentOrder.Quota = 1;                 // "your product quota"

            var alipayUtils = new AliPayUtils();
            return alipayUtils.GeneratePaymentInfo(paymentOrder);
        }
        #endregion

        #region [支付宝-App支付结果异步通知]
        /// <summary>
        /// https://doc.open.alipay.com/docs/doc.htm?spm=a219a.7629140.0.0.LWyWoI&treeId=204&articleId=105301&docType=1#s6
        /// </summary>
        /// <returns></returns>
        public string Alipay_Notify(HttpRequestBase request)
        {
            // 所有参数转成字典
            IDictionary<string, string> paramsMap = request.Form.AllKeys.ToDictionary(o => o, o => request.Form[o]);
            
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
    }
}
