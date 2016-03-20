using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    internal class AggregationLearningTests
    {
        private readonly IElasticClient _client = TestFactory.CreateClient();

        [Test]
        public void Can_access_metadata_of_termsAgg()
        {
            var queryResults = _client.Search<Movie>(s => s
                .From(0)
                .Size(10)
                .MatchAll()
                .Aggregations(a => a
                    .Terms("Year", t => t.Field(movie => movie.Year)))
                .Size(10)
            );

            var yearsAgg = queryResults.Aggs.Terms("Year");
            // test data contains two movies from 1962
            yearsAgg.Should().NotBeNull();
            var bucket = yearsAgg.Buckets.Single(b => b.Key == "1962");
            bucket.Key.Should().Be("1962");
            bucket.DocCount.Should().Be(2);
        }
    }
}