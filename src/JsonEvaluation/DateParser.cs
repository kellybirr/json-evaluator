using System;
using System.Globalization;
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
    }
}
