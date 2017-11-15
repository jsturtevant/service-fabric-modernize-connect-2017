using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WingtipToys.api
{
    public static class ShoppingCartHelpers
    {
        public static string GetPartionKey(this string s)
        {
            var md5 = MD5.Create();
            var value = md5.ComputeHash(Encoding.ASCII.GetBytes(s));
            return BitConverter.ToInt64(value, 0).ToString();
        }

        public static string BaseUrl
        {
            get
            {
                var reverseProxyIp = Environment.GetEnvironmentVariable("Fabric_NodeIPOrFQDN");

                if (string.IsNullOrEmpty(reverseProxyIp))
                {
                    reverseProxyIp = "localhost";
                }

                return $"http://{reverseProxyIp}:19081/WingtipToysApplication/ShoppingCart/";
            }
        }
    }
}