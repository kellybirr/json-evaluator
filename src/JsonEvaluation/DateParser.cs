using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Coderz.Json.Evaluation
{
    public static class DateParser
    {
        public static DataValue<DateTime> DateOnly(JToken token) 
            => DateOnly(token, CultureInfo.InvariantCulture);

        public static DataValue<DateTime> DateOnly(JToken token, CultureInfo culture)
        {
            DataValue<DateTimeOffset> value = DateAndTime(token, culture);
            return (value.HasValue)
                ? new DataValue<DateTime>(value.Value.Date)
                : new DataValue<DateTime>(false);
        }

        public static DataValue<DateTimeOffset> DateAndTime(JToken token) =>
            DateAndTime(token, CultureInfo.InvariantCulture);

        public static DataValue<DateTimeOffset> DateAndTime(JToken token, CultureInfo culture)
        {
            string tokenStr = token?.ToString();
            if (string.IsNullOrWhiteSpace(tokenStr)) 
                return new DataValue<DateTimeOffset>(false);

            if (DateTimeOffset.TryParse(tokenStr, culture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dto) || 
                DateTimeOffset.TryParse(tokenStr, culture.DateTimeFormat, DateTimeStyles.RoundtripKind, out dto) 
            ) return new DataValue<DateTimeOffset>(dto);

            return new DataValue<DateTimeOffset>(false);
        }

        public static DataValue<TimeSpan> Duration(JToken token) 
            => Duration(token, CultureInfo.InvariantCulture);

        public static DataValue<TimeSpan> Duration(JToken token, CultureInfo culture)
        {
            string tokenStr = token?.ToString();
            if (string.IsNullOrWhiteSpace(tokenStr)) 
                return new DataValue<TimeSpan>(false);

            if (tokenStr.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                return Iso8601Duration(tokenStr);

            if (TimeSpan.TryParse(tokenStr, culture.DateTimeFormat, out TimeSpan ts))
                return new DataValue<TimeSpan>(ts);

            return new DataValue<TimeSpan>(false);
        }

        private static DataValue<TimeSpan> Iso8601Duration(string s)
        {
            Match m = Ido8601Regex.Match(s);
            if (! m.Success) throw new FormatException("Invalid ISO-8601 Duration");

            // parse bits
            int.TryParse(m.Groups["days"]?.Value, out int days);
            int.TryParse(m.Groups["hours"]?.Value, out int hours);
            int.TryParse(m.Groups["minutes"]?.Value, out int minutes);
            int.TryParse(m.Groups["seconds"]?.Value, out int seconds);

            return new DataValue<TimeSpan>(new TimeSpan(days, hours, minutes, seconds));
        }

        // always CultureInvariant & IgnoreCase (acceptable limitation)
        private static readonly Regex Ido8601Regex = new Regex( // no years or months allowed
                @"^P(?:(?<days>\d+)D)?(?:T(?:(?<hours>\d+)H)?(?:(?<minutes>\d+)M)?(?:(?<seconds>\d+)S)?)?$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled
                );
    }
}
