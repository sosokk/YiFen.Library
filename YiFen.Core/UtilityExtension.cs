using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Security.Cryptography;
using System.IO;

namespace YiFen.Core
{
    public static class UtilityExtension
    {
        public static long ToLongSafely(this object o)
        {
            long x = 0;
            if (o == null) return x;
            long.TryParse(o.ToString(), out x);
            return x;
        }
        public static int ToIntSafely(this object o)
        {
            int x = 0;
            if (o == null) return x;
            int.TryParse(o.ToString(), out x);
            return x;
        }
        public static decimal ToDecimalSafely(this object o)
        {
            decimal x = 0;
            if (o == null) return x;
            decimal.TryParse(o.ToString(), out x);
            return x;
        }

        public static DateTime ToDateTimeSafely(this object o)
        {
            DateTime x = DateTime.MinValue;
            if (o == null) return x;
            DateTime.TryParse(o.ToString(), out x);
            return x;
        }
        public static Guid ToGuidSafely(this object o)
        {
            try
            {
                return new Guid(o.ToString());
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }

        public static string ToMD5(this string str)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
        }

        public static Guid ToNewGuid(this string str)
        {
            return new Guid(FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5"));
        }

        #region 对称加密算法

        /// <summary>
        /// 对称算法加密字符串。如果Key=null或者IV=null，那么使用默认的Key和IV。
        /// </summary>
        /// <param name="valueTobeEncrypt">需要加密的字符串</param>
        /// <param name="key">机密密钥</param>
        /// <param name="iv">初始化向量</param>
        /// <returns>加密后的字符串</returns>
        public static string SymmetricEncrypt(this string valueTobeEncrypt, string key = null, string iv = null)
        {
            SymmetricAlgorithm csp = GenProvider(key, iv);

            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            ct = csp.CreateEncryptor(csp.Key, csp.IV);

            byt = Encoding.UTF8.GetBytes(valueTobeEncrypt);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// 对称算法解密字符串。如果Key=null或者IV=null，那么使用默认的Key和IV。
        /// </summary>
        /// <param name="valueTobeDecrypt">需要解密的字符串</param>
        /// <param name="key">机密密钥</param>
        /// <param name="IV">初始化向量</param>
        /// <returns>解密后的字符串</returns>
        public static string SymmetricDecrypt(this string valueTobeDecrypt, string key = null, string IV = null)
        {
            SymmetricAlgorithm csp = GenProvider(key, IV);

            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;

            ct = csp.CreateDecryptor(csp.Key, csp.IV);

            byt = Convert.FromBase64String(valueTobeDecrypt);

            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();

            cs.Close();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        /// <summary>
        /// 产生密钥。
        /// </summary>
        /// <returns></returns>
        private static string GenKey()
        {
            SymmetricAlgorithm csp = new DESCryptoServiceProvider();
            csp.GenerateKey();
            return Convert.ToBase64String(csp.Key);
        }

        /// <summary>
        /// 产生向量。
        /// </summary>
        /// <returns></returns>
        private static string GenIv()
        {
            SymmetricAlgorithm csp = new DESCryptoServiceProvider();
            csp.GenerateIV();
            return Convert.ToBase64String(csp.IV);
        }

        private static SymmetricAlgorithm GenProvider(string key, string iv)
        {
            SymmetricAlgorithm decCSP = new DESCryptoServiceProvider();
            if (key == null)
            {
                key = System.Configuration.ConfigurationManager.AppSettings["DefaultKey"];
                iv = System.Configuration.ConfigurationManager.AppSettings["DefaultIV"];
            }

            decCSP.Key = Convert.FromBase64String(key ?? "Ga7ODN4NK18=");
            decCSP.IV = Convert.FromBase64String(iv ?? "gJ4yTFfD38Y=");

            return decCSP;
        }

        #endregion
    }
}
