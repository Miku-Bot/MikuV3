using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MikuV3.Music.Utilities
{
    public class TimeConversion
    {
        public static DateTime ParseYTDLDate(string date)
        {
            var s = date.ToCharArray();
            Console.WriteLine(date);
            return new DateTime(Convert.ToInt32($"{s[0]}{s[1]}{s[2]}{s[3]}"), Convert.ToInt32($"{s[4]}{s[5]}"), Convert.ToInt32($"{s[6]}{s[7]}"));
        }
    }
}
