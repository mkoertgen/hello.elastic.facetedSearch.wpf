using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
    // ReSharper disable InconsistentNaming
    [TestFixture, Explicit]
    class FacetLearningTests
    {
        private readonly IElasticClient _client = TestFactory.CreateClient();

        [Test]
        public void Can_access_metadata_of_termFacet()
        {
            var queryResults = _client.Search<Movie>(s => s
                .From(0)
                .Size(10)
                .MatchAll()
                .FacetTerm(term => term
                  .OnField(movie => movie.Year)
                  .Size(10)
                )
            );

            //var termFacet = (TermFacet)queryResults.Facets.First().Value;
            //termFacet.Should().NotBeNull();
            var yearsFacet = queryResults.Facet<TermFacet>(p=>p.Year);
            // test data contains two movies from 1962
            var termItem = yearsFacet.Items.First();
            termItem.Term.Should().Be("1962");
            termItem.Count.Should().Be(2);
        }
    }
}