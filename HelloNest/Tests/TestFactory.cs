using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using static Nest.Infer;

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

		private static IElasticClient CreateClient(IConnectionSettingsValues settings)
		{
			var client = new ElasticClient(settings);

			client.RootNodeInfo().IsValid.Should().BeTrue();

			IndexTestMovies(client);

			return client;

		}

		internal static IElasticClient CreateClient()
		{
			var node = new Uri("http://localhost:9200");

			var connectionPool = new SingleNodeConnectionPool(node);
			var connectionSettings = new ConnectionSettings(connectionPool)
				.DefaultIndex("movies");

			return CreateClient(connectionSettings);
		}

		private static void IndexTestMovies(IElasticClient client)
		{
			if (!client.IndexExists(Indices<Movie>()).Exists)
			{
				client.CreateIndex(Index<Movie>(), c => c
					.Mappings(map => map
						.Map<Movie>(m => m
							.AutoMap()
						)
					)
				);
			}

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
			response.Items.Should().OnlyContain(i => i.IsValid && !string.IsNullOrEmpty(i.Id));
		}
	}
}