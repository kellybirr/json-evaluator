using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public static class DateParser
    {
        public static DateTime DateOnly(JToken token) => DateAndTime(token).Date;

        public static DateTimeOffset DateAndTime(JToken token)
        {
            string tokenStr = token?.ToString();
            if (string.IsNullOrWhiteSpace(tokenStr))
                throw new ArgumentOutOfRangeException(nameof(token));

            if (DateTimeOffset.TryParse(tokenStr, out DateTimeOffset dto))
                return dto;

            if (DateTimeOffset.TryParse(tokenStr, null, DateTimeStyles.RoundtripKind, out dto))
                return dto;

            // not TryParse() - we want exception on fail
            DateTime dt = DateTime.Parse(tokenStr);
            return new DateTimeOffset(dt, TimeSpan.Zero);
        }

        public static TimeSpan Duration(JToken token)
        {
            string tokenStr = token?.ToString();
            if (string.IsNullOrWhiteSpace(tokenStr))
                throw new ArgumentOutOfRangeException(nameof(token));

            if (tokenStr.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                return Iso8601Duration(tokenStr);

            return TimeSpan.Parse(tokenStr);
        }

        private static TimeSpan Iso8601Duration(string s)
        {
            Match m = Ido8601Regex.Match(s);
            if (! m.Success) throw new FormatException("Invalid ISO-8601 Duration");

            // parse bits
            int.TryParse(m.Groups["days"]?.Value, out int days);
            int.TryParse(m.Groups["hours"]?.Value, out int hours);
            int.TryParse(m.Groups["minutes"]?.Value, out int minutes);
            int.TryParse(m.Groups["seconds"]?.Value, out int seconds);

            return new TimeSpan(days, hours, minutes, seconds);
        }

        private static readonly Regex Ido8601Regex = new Regex( // no years or months allowed
                @"^P(?:(?<days>\d+)D)?(?:T(?:(?<hours>\d+)H)?(?:(?<minutes>\d+)M)?(?:(?<seconds>\d+)S)?)?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
                );
    }
}
