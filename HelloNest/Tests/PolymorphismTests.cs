using System;
using System.Collections.Generic;
using Elasticsearch.Net.Connection;
using FluentAssertions;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HelloNest.Tests
{
	// ReSharper disable InconsistentNaming
	[TestFixture, Explicit]
	class PolymorphismTests
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

		static IElasticClient CreateClient(IConnectionSettingsValues settings, IConnection connection)
		{
			var client = new ElasticClient(settings, connection);

			client.RootNodeInfo().IsValid.Should().BeTrue();
			IndexTestAlbums(client);
			return client;

		}

		internal static IElasticClient CreateClient()
		{
			var settings = new ConnectionSettings(new Uri("http://localhost:9200"), "albums")
				.UsePrettyResponses()
				.AddContractJsonConverters(type =>
										   {
											   if (type == typeof(TrackList))
												   return new JsonCreationConverter<TrackList>();
											   return null;
										   });
			var connection = new HttpConnection(settings); // new InMemoryConnection(settings);
			return CreateClient(settings, connection);
		}

		private static void IndexTestAlbums(ElasticClient client)
		{
			// insert
			var response = client.IndexMany(TestAlbums);
			response.IsValid.Should().BeTrue();
			response.Items.Should().NotBeEmpty();
			response.Items.Should().OnlyContain(i => i.IsValid && !String.IsNullOrEmpty(i.Id));

			// assert index exists / has been created
			client.IndexExists(i => i.Index("albums")).Exists.Should().BeTrue();
		}
	}

	public interface IHaveType { string Type { get; } }

	public class JsonCreationConverter<T> : JsonConverter where T : IHaveType, new()
	{
		public override bool CanConvert(Type objectType) { return typeof(T).IsAssignableFrom(objectType); }

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jObject = JObject.Load(reader);

			// Create target object based on JObject
			T target = Create(objectType, jObject);

			// Populate the object properties
			serializer.Populate(jObject.CreateReader(), target);

			return target;
		}

		private T Create(Type objectType, JObject jObject)
		{
			var typeName = (string)(jObject["type"] ?? objectType.AssemblyQualifiedName);
			var type = Type.GetType(typeName) ?? typeof(T);
			return (T)Activator.CreateInstance(type);
		}


		public override bool CanWrite { get { return false; } }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}