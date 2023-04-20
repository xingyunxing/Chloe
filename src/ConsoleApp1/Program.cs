using DotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string appKey = "73cf8b63a6554b1ebe67888e41b31cca";
            string appSecret = "487e20127ee4510418a147b314e1b40a";
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["appKey"] = appKey;
            parameters["appSecret"] = appSecret;
            string res = HttpHelper.Post("http://open.ys7.com/api/lapp/token/get", parameters, 10000);



            
        }
    }
}
