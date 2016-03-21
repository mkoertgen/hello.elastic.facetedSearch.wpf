using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
    [TestFixture, Explicit]
    internal class UpdateTests
    {
        private readonly IElasticClient _client = TestFactory.CreateClient();

        [Test]
        public void Can_do_partial_updates_even_on_nested_fields()
        {
            var getResponse = _client.Get<Movie>(1);
            var movie = getResponse.Source;


            // update a simple field
            var response = _client.Update<Movie, object>(new DocumentPath<Movie>(1), u => u.Doc(new { Year = movie.Year + 1 }));
            response.Version.Should().Be(getResponse.Version + 1);

            getResponse = _client.Get<Movie>(movie.Id);
            getResponse.Source.Year.Should().Be(movie.Year + 1);
            getResponse.Version.Should().Be(response.Version);

            // update a nested field
            _client.Update<Movie, object>(new DocumentPath<Movie>(1), u => u.Doc(new { Actors = new[] { new Person { Name = "Robert", Age = 25 } } }));
            _client.Get<Movie>(movie.Id).Source.Actors.Single().Age.Should().Be(25);

            // "clear" updates
            _client.Index(movie);
        }
    }
}