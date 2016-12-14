using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Kip.Utils.Core.Test
{
    class TestX509
    {
        #region [用私钥解密数据]
        public static string DecryptByPrivateKey(string encryptStr)
        {
            // 声明根目录
            string rootPath = ""; //    Server.MapPath("/App_Config/")
            // 从pfx文件中获取私钥，并声明为可导出
            X509Certificate2 cer2 = new X509Certificate2(rootPath + "X509_private_key.pfx", "nYq98GqsNKy4zDv1", X509KeyStorageFlags.Exportable);
            string privateKey = cer2.PrivateKey.ToXmlString(true);

            // 通过私钥解密数据
            return SecurityUtils.RSADecrypt(privateKey, encryptStr);
        }
        #endregion
    }
}
