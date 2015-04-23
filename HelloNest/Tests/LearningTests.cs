using System.Globalization;
using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
	// ReSharper disable InconsistentNaming
	[TestFixture, Explicit]
	class LearningTests
	{
		private readonly IElasticClient _client = TestFactory.CreateClient();

		
		[Test]
		public void Can_do_partial_updates_even_on_nested_fields()
		{
			var getResponse = _client.Get<Movie>(1);
			var movie = getResponse.Source;


			// update a simple field
			var response = _client.Update<Movie, object>(u => u.Id(movie.Id).Doc(new {Year = movie.Year + 1}));
			response.Version.Should().Be((int.Parse(getResponse.Version) + 1).ToString(CultureInfo.InvariantCulture));

			getResponse = _client.Get<Movie>(movie.Id);
			getResponse.Source.Year.Should().Be(movie.Year + 1);
			getResponse.Version.Should().Be(response.Version);

			// update a nested field
			_client.Update<Movie, object>(u => u.Id(movie.Id).Doc(new { Actors = new[]{new Person{Name="Robert", Age=25}}}));
			_client.Get<Movie>(movie.Id).Source.Actors.Single().Age.Should().Be(25);

			// "clear" updates
			_client.Index(movie);
		}
	}
}
