using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Extensions
{
    internal static class StringExtensions
    {
        public static string Truncate(this string input, int length)
        {
            if(string.IsNullOrEmpty(input)) return input;
            if(input.Length <= length) return input;
            else return input.Substring(0, length);
        }
    }
}
