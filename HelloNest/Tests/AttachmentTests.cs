using System;
using Nest;
using NUnit.Framework;
using static Nest.Infer;

namespace HelloNest.Tests
{
	// cf.: http://stackoverflow.com/questions/14781946/elasticsearch-attachment-type-nest-c
	[TestFixture]
	internal class AttachmentTests
	{
		[Test]
		public void TestAttachments()
		{
			var client = CreateClient();

			var expected = new Document
			{
				File = Attachment.CreateFromFile("elasticsearch.png")
			};
			client.Index(expected);

			//var query = Query<Document>.Term("file", "searchTerm");

			var actual = client.Get<Document>(expected.Id.ToString()).Source;
			actual.File.SaveToFile("elasticsearch2.png");
		}

		private static IElasticClient CreateClient()
		{
			var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
				.DefaultIndex("documents");

			var client = new ElasticClient(settings);

			if (!client.IndexExists(Indices<Document>()).Exists)
				client.CreateIndex(Index<Document>(), c => c
					.Mappings(map => map
						.Map<Document>(m => m.AutoMap()))
				);

			return client;
		}
	}
}