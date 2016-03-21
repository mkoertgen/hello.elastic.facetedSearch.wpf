using System;

namespace HelloAggregations.Tests
{
    internal static class ElasticSearchExtensions
    {
        // cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/mapping-core-types.html#date
        private static readonly DateTime Epoch = new DateTime(1970,1,1,0,0,0,0,DateTimeKind.Utc);

        public static DateTime ToDateTime(this double value)
        {
            return Epoch + TimeSpan.FromMilliseconds(value);
        }

        public static DateTime? ToDateTime(this double? value)
        {
            return value.HasValue ? (DateTime?) value.Value.ToDateTime() : null;
        }
    }
}