using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kip.Utils.Payment.AliPay
{
    public class AliPayConfig
    {
        //应用公钥、私钥文件
        public static readonly string merchant_private_key = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Config/alipay_rsa_private_key.pem");
        public static readonly string merchant_public_key = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Config/alipay_rsa_public_key.pem");
        // 支付宝公钥文件
        public static readonly string alipay_merchant_public_key = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Config/alipay_merchant_rsa_public_key.pem");

        public static readonly string charset = "utf-8";

        public static readonly string seller_id = "your_seller_id";
        public static readonly string notify_url = "your_notify_url";

        // -----沙箱环境-----
        //public static readonly string server_url = "http://openapi.alipaydev.com/gateway.do";
        //public static readonly string app_id = "your_app_id";

        // -----线上地址-----
        public static readonly string server_url = "https://openapi.alipay.com/gateway.do";
        public static readonly string app_id = "your_app_id";

        public AliPayConfig()
        {
            //
            // TODO: Add constructor logic here
            //

        }

        public static string GetMerchantPublicKeyStr()
        {
            StreamReader sr = new StreamReader(merchant_public_key);
            string pubkey = sr.ReadToEnd();
            sr.Close();
            if (pubkey != null)
            {
                pubkey = pubkey.Replace("-----BEGIN PUBLIC KEY-----", "");
                pubkey = pubkey.Replace("-----END PUBLIC KEY-----", "");
                pubkey = pubkey.Replace("\r", "");
                pubkey = pubkey.Replace("\n", "");
            }
            return pubkey;
        }
    }
}
