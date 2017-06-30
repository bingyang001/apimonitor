using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ApiInterceptionHttpmodule.utils
{
    public class CryptographyUtils
    {
        public static String encryption(String val)
        {
            if (String.IsNullOrEmpty(val))
            {
                throw new ArgumentNullException("val is null or empty.");
            }

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] by = Encoding.GetEncoding("UTF-8").GetBytes(val);
            byte[] output = md5.ComputeHash(by);
            String result = BitConverter.ToString(output).Replace("-", "");
            return result;
        }
    }
}
