using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloAggregations.Tests
{
    // cf.: http://blog.qbox.io/elasticsearch-aggregations
    [TestFixture, Explicit]
    internal class LearningTests
    {
        private readonly IElasticClient _client = TestFactory.CreateClient();

        [Test, Description("A test grouping (bucketing) the athletes by sport")]
        public void Test_simple_bucket()
        {
            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .Terms("sport", t => t.Field(athlete => athlete.Sport))
                )
            );

            results.Total.Should().Be(TestFactory.Athletes.Count);

            var buckets = results.Aggs.Terms("sport").Buckets;
            buckets.Should().NotBeNull();

            buckets.Should().HaveCount(5);

            buckets[0].Key.Should().Be("baseball");
            buckets[0].DocCount.Should().Be(16);

            buckets[1].Key.Should().Be("football");
            buckets[1].DocCount.Should().Be(2);

            buckets[2].Key.Should().Be("golf");
            buckets[2].DocCount.Should().Be(2);

            buckets[3].Key.Should().Be("basketball");
            buckets[3].DocCount.Should().Be(1);

            buckets[4].Key.Should().Be("hockey");
            buckets[4].DocCount.Should().Be(1);
        }

        [Test]//, Ignore("\"Nested\" does not work on simple rating array")]
        public void Test_nested_rating_average()
        {
            // cf.: http://www.elasticsearch.org/blog/introducing-elasticsearch-net-nest-1-0-0-beta1/

            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .Terms("name", t => t.Field(m => m.Name)
                        //.OrderDescending("rating_avg")
                    )
                    .Nested("rating", n => n
                        .Path(p => p.Rating)
                        .Aggregations(aa => aa
                            .Average("rating_avg", m => m.Field(p => p.Rating))
                        )
                    )
                )
            );

            var names = results.Aggs.Terms("name");
            names.Buckets.Should().HaveCount(10);

            var rating = results.Aggs.Nested("rating");

            rating.Should().NotBeNull();
            // TODO...
            //var averageRating = bucket.Nested("rating").Average("avg_rating");

        }

        [Test, Description("Search all athletes within a 20 miles radius around a point")]
        public void Test_geo_distance()
        {
            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .GeoDistance("baseball_player_ring", g => g
                        .Field(f => f.Location)
                        .Origin(46.12, -68.55)
                        .Unit(DistanceUnit.Miles)
                        .Ranges(r => r.From(0).To(20)))
                    )
                );

            var buckets = results.Aggs.GeoDistance("baseball_player_ring").Buckets;

            var rangeItem = buckets.Single();
            rangeItem.DocCount.Should().Be(14, "14 athletes should live within a 20mi radius");
        }

        [Test]
        public void Test_rating_stats()
        {
            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .Stats("stats_rating", r => r
                        .Field(f => f.Rating))));

            var stats = results.Aggs.Stats("stats_rating");

            stats.Min.Should().Be(1);
            stats.Max.Should().Be(10);
            stats.Average.Should().BeInRange(4.60, 4.62);
            stats.Count.Should().Be(44);
            stats.Sum.Should().Be(203);
        }

        [Test]
        public void Test_date_range_aggregation()
        {
            const string fmtDate = "yyyy-MM-dd";
            // cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/master/search-aggregations-bucket-daterange-aggregation.html

            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .DateRange("date_ranges", dr => dr
                        .Field(f => f.BirthDate)
                        .Format(fmtDate)
                        .Ranges(r => r.From("now-1d").To("now"), // today
                            r => r.From("now-2d").To("now-1d"), // yesterday
                            r => r.To("now-2d") // all before yesterday
                            )
                        )
                    )
                );

            var buckets = results.Aggs.DateRange("date_ranges").Buckets;

            buckets.Should().HaveCount(3);

            var rangeItem = buckets[0];
            rangeItem.DocCount.Should().Be(22, "all athletes were born long before two days");

            var to = rangeItem.To.ToDateTime();
            to.Should().BeBefore(DateTime.UtcNow - TimeSpan.FromDays(2));

            var toFromString = DateTime.ParseExact(rangeItem.ToAsString, fmtDate, CultureInfo.InvariantCulture);
            // ReSharper disable once PossibleInvalidOperationException
            toFromString.Should().Be(to.Value.Date);
        }
    }
}
