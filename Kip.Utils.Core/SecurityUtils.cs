using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace Kip.Utils.Core
{
    public static class SecurityUtils
    {
        public static string HashPassword(string password)
        {
            string passwordPrefix = "prefix"; //用于密码加密的前缀
            return FormsAuthentication.HashPasswordForStoringInConfigFile(passwordPrefix + password, "MD5");
        }

        #region DES加密解密
        /// <summary>
        /// DES加密字符串
        /// </summary>
        /// <param name="encryptString">待加密的字符串</param>
        /// <param name="salt">盐值</param>
        /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public static string DESEncrypt(string encryptString, string salt)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(salt.Substring(0, 8));
                byte[] rgbIV = rgbKey;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);

                using (DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider())
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        using (CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            return Convert.ToBase64String(mStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return encryptString;
            }
        }

        /// <summary>
        /// DES解密字符串
        /// </summary>
        /// <param name="decryptString">待解密的字符串</param>
        /// <param name="salt">盐值</param>
        /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
        public static string DESDecrypt(string decryptString, string salt)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(salt.Substring(0, 8));
                byte[] rgbIV = rgbKey;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);

                using (DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider())
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        using (CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write))
                        {
                            cStream.Write(inputByteArray, 0, inputByteArray.Length);
                            cStream.FlushFinalBlock();
                            return Encoding.UTF8.GetString(mStream.ToArray());
                        }
                    }
                }
            }
            catch
            {
                return decryptString;
            }
        }
        #endregion

        #region [MD5加密]
        public static string Md5Encrypt(string source)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //获取密文字节数组
            byte[] bytResult = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(source));

            //转换成字符串，32位
            string strResult = BitConverter.ToString(bytResult);

            //BitConverter转换出来的字符串会在每个字符中间产生一个分隔符，需要去除掉
            return strResult.Replace("-", "");
        }
        #endregion

        #region [RSA不对称加解密]
        /// <summary>  
        /// RSA加密  
        /// </summary>  
        /// <param name="xmlPublicKey">公钥</param>  
        /// <param name="m_strEncryptString">MD5加密后的数据</param>  
        /// <returns>RSA公钥加密后的数据</returns>  
        public static string RSAEncrypt(string xmlPublicKey, string m_strEncryptString)
        {
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(xmlPublicKey);

                Byte[] plaintextData = Encoding.UTF8.GetBytes(m_strEncryptString);
                int maxBlockSize = provider.KeySize / 8 - 11;    //加密块最大长度限制

                if (plaintextData.Length <= maxBlockSize)
                    return Convert.ToBase64String(provider.Encrypt(plaintextData, false));

                using (MemoryStream plaiStream = new MemoryStream(plaintextData))
                using (MemoryStream crypStream = new MemoryStream())
                {
                    Byte[] buffer = new Byte[maxBlockSize];
                    int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);

                    while (blockSize > 0)
                    {
                        Byte[] toEncrypt = new Byte[blockSize];
                        Array.Copy(buffer, 0, toEncrypt, 0, blockSize);

                        Byte[] cryptograph = provider.Encrypt(toEncrypt, false);
                        crypStream.Write(cryptograph, 0, cryptograph.Length);

                        blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                    }

                    return Convert.ToBase64String(crypStream.ToArray(), Base64FormattingOptions.None);
                }
            }
        }

        /// <summary>  
        /// RSA解密  
        /// </summary>  
        /// <param name="xmlPrivateKey">私钥</param>  
        /// <param name="m_strDecryptString">待解密的数据</param>  
        /// <returns>解密后的数据</returns>  
        public static string RSADecrypt(string xmlPrivateKey, string m_strDecryptString)
        {
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(xmlPrivateKey);

                Byte[] ciphertextData = Convert.FromBase64String(m_strDecryptString);
                int maxBlockSize = provider.KeySize / 8;    //解密块最大长度限制

                if (ciphertextData.Length <= maxBlockSize)
                    return Encoding.UTF8.GetString(provider.Decrypt(ciphertextData, false));

                using (MemoryStream crypStream = new MemoryStream(ciphertextData))
                using (MemoryStream plaiStream = new MemoryStream())
                {
                    Byte[] buffer = new Byte[maxBlockSize];
                    int blockSize = crypStream.Read(buffer, 0, maxBlockSize);

                    while (blockSize > 0)
                    {
                        Byte[] toDecrypt = new Byte[blockSize];
                        Array.Copy(buffer, 0, toDecrypt, 0, blockSize);

                        Byte[] plaintext = provider.Decrypt(toDecrypt, false);
                        plaiStream.Write(plaintext, 0, plaintext.Length);

                        blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                    }

                    return Encoding.UTF8.GetString(plaiStream.ToArray());
                }
            }
        }
        #endregion

        #region [RSA签名验证]
        public static string RSASign(string xmlPrivateKey, string srcString)
        {
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(xmlPrivateKey);

                Byte[] dataToSign = Encoding.UTF8.GetBytes(srcString);
                Byte[] result = provider.SignData(dataToSign, new SHA1CryptoServiceProvider());

                return Convert.ToBase64String(result);
            }
        }
        public static bool RSAVerifySigned(string xmlPublicKey, string srcString, string signedString)
        {
            using (RSACryptoServiceProvider provider = new RSACryptoServiceProvider())
            {
                provider.FromXmlString(xmlPublicKey);

                Byte[] srcData = Encoding.UTF8.GetBytes(srcString);
                Byte[] signedData = Encoding.UTF8.GetBytes(signedString);
                return provider.VerifyData(srcData, new SHA1CryptoServiceProvider(), signedData);
            }
        }
        #endregion

        #region [SHA1加密]
        public static string GetSHA1HashString(string input)
        {
            var output = string.Empty;
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var inputBytes = ASCIIEncoding.ASCII.GetBytes(input);
                var outputBytes = sha1.ComputeHash(inputBytes);
                return BitConverter.ToString(outputBytes).Replace("-", "");
            }
        }
        #endregion

        #region [生成随机字母与数字或字符]
        /// <summary>
        /// 生成随机字母与数字或字符
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <returns></returns>
        public static string GenerateRandomStr(int Length)
        {
            // 在生成前将当前线程阻止以避免重复
            System.Threading.Thread.Sleep(3);

            char[] Pattern = new char[] { 
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', 
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
            string result = "";
            int n = Pattern.Length;
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];

            }
            return result;
        }
        #endregion
    }
}
