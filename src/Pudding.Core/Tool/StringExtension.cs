using System;
using System.Security.Cryptography;
using System.Text;

namespace Pudding.Core.Tool
{
    public static class StringExtension
    {
        public static string GetSubstring(this string main, int start)
        {
            return main.AsSpan().Slice(start).ToString();
        }

        public static string GetSubstring(this string main, int start, int length)
        {
            return main.AsSpan().Slice(start, length).ToString();
        }

        public static string GetMd5(this string str)
        {
            byte[] textBytes = Encoding.Default.GetBytes(str);
            MD5CryptoServiceProvider cryptHandler = new MD5CryptoServiceProvider();
            byte[] hash = cryptHandler.ComputeHash(textBytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte a in hash.AsSpan())
            {
                if (a < 16)
                {
                    if (a < 10)
                    {
                        builder.Append(a);
                    }
                    else
                    {
                        builder.Append(a - 10);
                    }

                    builder.Append(a.ToString("x"));
                }
                else
                {
                    builder.Append(a.ToString("x"));
                }
            }
            return builder.ToString();
        }
    }
}
