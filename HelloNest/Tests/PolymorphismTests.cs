using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;
using NUnit.Framework;
using static Nest.Infer;

namespace HelloNest.Tests
{
	// ReSharper disable InconsistentNaming
	[TestFixture]
	internal class PolymorphismTests
	{
		private readonly IElasticClient _client = CreateClient();

		[Test]
		public void Serialization_should_support_polymorhpism()
		{
			var album = _client.Get<Album>(1).Source;
			album.Tracks.Should().BeOfType<CdTrackList>();

			album = _client.Get<Album>(2).Source;
			album.Tracks.Should().BeOfType<LpTrackList>();

			album = _client.Get<Album>(3).Source;
			album.Tracks.Should().BeOfType<TrackList>();
		}

		internal static readonly List<Album> TestAlbums = new List<Album>
			{
				new Album
				{
					Id = 1,
					Name="Seine grössten Erfolge", Artist= "Helge Schneider",
					Tracks = new CdTrackList { Tracks = "Der dreieckige Trompeter"}
				},
				new Album
				{
					Id = 2,
					Name="Die Bestie in Menschengestalt", Artist= "Die Ärzte",
					Tracks = new LpTrackList { TracksSideA = "Der Schunda-Song", TracksSideB = "Mit dem Schwert nach Polen"}
				},

				new Album
				{
					Id = 3,
					Tracks = new TrackList()
				},

			};

		private static IElasticClient CreateClient(IConnectionSettingsValues settings)
		{
			var client = new ElasticClient(settings);

			client.RootNodeInfo().IsValid.Should().BeTrue();

			IndexTestAlbums(client);

			return client;

		}

		internal static IElasticClient CreateClient()
		{
			var node = new Uri("http://localhost:9200");

			var connectionPool = new SingleNodeConnectionPool(node);

			var settings = new ConnectionSettings(connectionPool, c => new TracklistJsonNetSerializer(c))
				.DefaultIndex("albums")
				.PrettyJson();

			return CreateClient(settings);
		}

		private static void IndexTestAlbums(IElasticClient client)
		{
			if (!client.IndexExists(Indices<Album>()).Exists)
				client.CreateIndex(Index<Album>(), c => c
					.Mappings(map => map
						.Map<Document>(m => m.AutoMap()))
				);

			// insert
			var response = client.IndexMany(TestAlbums);
			response.IsValid.Should().BeTrue();
			response.Items.Should().NotBeEmpty();
			response.Items.Should().OnlyContain(i => i.IsValid && !string.IsNullOrEmpty(i.Id));
		}
	}
}