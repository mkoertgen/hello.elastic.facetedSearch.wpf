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

            var bucket = results.Aggs.Terms("sport");
            bucket.Should().NotBeNull();

            bucket.Items.Should().HaveCount(5);

            bucket.Items[0].Key.Should().Be("baseball");
            bucket.Items[0].DocCount.Should().Be(16);

            bucket.Items[1].Key.Should().Be("football");
            bucket.Items[1].DocCount.Should().Be(2);

            bucket.Items[2].Key.Should().Be("golf");
            bucket.Items[2].DocCount.Should().Be(2);

            bucket.Items[3].Key.Should().Be("basketball");
            bucket.Items[3].DocCount.Should().Be(1);

            bucket.Items[4].Key.Should().Be("hockey");
            bucket.Items[4].DocCount.Should().Be(1);
        }

        [Test, Ignore("\"Nested\" does not work on simple rating array")]
        public void Test_nested_rating_average()
        {
            // cf.: http://www.elasticsearch.org/blog/introducing-elasticsearch-net-nest-1-0-0-beta1/

            var results = _client.Search<Athlete>(s => s
                .Size(0)
                .Aggregations(a => a
                    .Terms("name", t => t.Field(m => m.Name))
                        .Nested("rating", n => n
                            .Path(p => p.Rating)
                            .Aggregations(r => r
                                .Average("avg_rating", m=>m.Field(p=>p.Rating.First()))
                            )
                        )
                    )
                );

            var bucket = results.Aggs.Nested("name");
            var averageRating = bucket.Average("avg_rating");

            // TODO: ...
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
                        .Unit(GeoUnit.Miles)
                        .Ranges(r => r.From(0).To(20)))
                    )
                );

            var bucket = results.Aggs.GeoDistance("baseball_player_ring");

            var rangeItem = bucket.Items.Single();
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
            stats.Average.Should().BeInRange(4.37, 4.38);
            stats.Count.Should().Be(37);
            stats.Sum.Should().Be(162);
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

            var bucket = results.Aggs.DateRange("date_ranges");

            bucket.Items.Should().HaveCount(3);

            var rangeItem = bucket.Items[0];
            rangeItem.DocCount.Should().Be(22, "all athletes were born long before two days");

            var to = rangeItem.To.ToDateTime();
            to.Should().BeBefore(DateTime.UtcNow - TimeSpan.FromDays(2));

            var toFromString = DateTime.ParseExact(rangeItem.ToAsString, fmtDate, CultureInfo.InvariantCulture);
            // ReSharper disable once PossibleInvalidOperationException
            toFromString.Should().Be(to.Value.Date);
        }
    }
}
