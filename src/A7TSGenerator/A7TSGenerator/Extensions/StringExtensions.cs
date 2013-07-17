using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A7TSGenerator.Extensions
{
    public static class StringExtensions
    {
        public static string CaptilizeFirstLetter(this string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
        }

        public static string LowerCaseFirstLetter(this string s)
        {
            return s.Substring(0, 1).ToLower() + s.Substring(1);
        }
    }
}
