using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
    [TestFixture, Explicit]
    class UpdateTests
    {
        private readonly IElasticClient _client = TestFactory.CreateClient();


        [Test]
        public void Can_do_a_partial_update()
        {
            var results = _client.Search<Movie>(s => s
                .From(0)
                .Size(10)
                //.Fields(m => m.Id, m => m.Title)
                .SortAscending(m => m.Year)
                .Query(q => q.Term(m => m.Director, "coppola"))
                );

            results.Documents.Should().HaveCount(2);
        }
        
    }
}