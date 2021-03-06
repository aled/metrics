using System;
using System.Collections.Generic;
using System.Linq;

namespace Wibblr.Metrics.Plugins.CockroachDb
{
    public static class Extensions
    {
        public static DateTime RoundDown(this DateTime dateTime, TimeSpan timeSpan) =>
            new DateTime(dateTime.Ticks - (dateTime.Ticks % timeSpan.Ticks), DateTimeKind.Utc);

        public static DateTime RoundUp(this DateTime dateTime, TimeSpan timeSpan) =>
            new DateTime(dateTime.Ticks - (dateTime.Ticks % timeSpan.Ticks) + timeSpan.Ticks, DateTimeKind.Utc);

        public static DateTime Seconds(this DateTime dateTime) =>
            dateTime.RoundDown(TimeSpan.FromSeconds(1));

        public static bool IsDivisorOf(this TimeSpan part, TimeSpan whole) =>
            whole.Ticks % part.Ticks == 0;

        public static IEnumerable<IEnumerable<T>> Batches<T>(this IEnumerable<T> items, int batchSize)
        {
            var batch = new List<T>();
            foreach (var item in items)
            {
                batch.Add(item);
                if (batch.Count == batchSize)
                    yield return batch;
            }
            if (batch.Count > 0)
                yield return batch;
        }

        public static IEnumerable<(T, int)> ZipWithIndex<T>(this IEnumerable<T> items)
        {
            var i = 0;
            foreach (var item in items)
                yield return (item, i++);
        }

        public static bool IsAlphanumeric(this string s) => 
            s.All(c => char.IsLetterOrDigit(c));

        public static bool IsEmpty<T>(this IEnumerable<T> items) =>
            !items.Any();

        public static string Join(this IEnumerable<string> items, string separator = ", ") =>
            string.Join(separator, items);

        public static string SqlQuote(this string s) => 
            "\"" + s.Replace("\"", "\\\"") + "\"";
    }
}
