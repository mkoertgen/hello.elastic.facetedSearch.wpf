using System;
using System.Collections.Generic;
using Elasticsearch.Net.Connection;
using FluentAssertions;
using Nest;

namespace HelloNest.Tests
{
	internal static class TestFactory
	{
	    private static readonly List<Movie> TestMovies = new List<Movie>
			{
				new Movie { Id = 1, Title = "The Godfather", 
					Director = "Francis Ford Coppola", 
					Genres = new[]{"Crime","Drama"}, 
					Year = 1972,
					Actors = new []{ new Person{Name="Robert", Age=24}}},
				new Movie { Id = 2, Title = "Lawrence of Arabia", 
					Director = "David Lean", 
					Genres = new[]{"Adventure","Biography","Drama"}, 
					Year = 1962},
				new Movie { Id = 3, Title = "To Kill a Mockingbird", 
					Director = "Robert Mulligan", 
					Genres = new[]{"Crime","Drama","Mystery"}, 
					Year = 1962},
				new Movie { Id = 4, Title = "The Assassination of Jesse James by the Coward Robert Ford", 
					Director = "Andrew Dominik", 
					Genres = new[]{"Biography","Crime","Drama"}, 
					Year = 2007},
				new Movie { Id = 5, Title = "Kill Bill: Vol. 1", 
					Director = "Quentin Tarantino", 
					Genres = new[]{"Action","Crime","Thriller"}, 
					Year = 2003},
				new Movie { Id = 6, Title = "Apocalypse Now", 
					Director = "Francis Ford Coppola", 
					Genres = new[]{"Drama","War"}, 
					Year = 1979}
			};

	    private static IElasticClient CreateClient(IConnectionSettingsValues settings, IConnection connection)
		{
			var client = new ElasticClient(settings, connection);

			client.RootNodeInfo().IsValid.Should().BeTrue();
			IndexTestMovies(client);
			return client;

		}

		internal static IElasticClient CreateClient()
		{
			var settings = new ConnectionSettings(new Uri("http://localhost:9200"), "movies").UsePrettyResponses();
			var connection = new HttpConnection(settings); // new InMemoryConnection(settings);
			return CreateClient(settings, connection);
		}

		private static void IndexTestMovies(IElasticClient client)
		{
			// update test data
			TestMovies.ForEach(movie =>
			{
				movie.Created = DateTime.UtcNow;
				movie.CreatedBy = Environment.UserName;
			});

			// insert
			var response = client.IndexMany(TestMovies);
			response.IsValid.Should().BeTrue();
			response.Items.Should().NotBeEmpty();
			response.Items.Should().OnlyContain(i => i.IsValid && !String.IsNullOrEmpty(i.Id));

			// assert index exists / has been created
			client.IndexExists(i => i.Index("movies")).Exists.Should().BeTrue();
		}
	}
}