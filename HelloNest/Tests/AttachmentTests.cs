using System;
using System.IO;
using Elasticsearch.Net.Connection;
using Nest;
using NUnit.Framework;

namespace HelloNest.Tests
{
	// cf.: http://stackoverflow.com/questions/14781946/elasticsearch-attachment-type-nest-c
	[TestFixture]
	class AttachmentTests
	{
		[Test, Explicit]
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
			var settings = new ConnectionSettings(new Uri("http://localhost:9200"), "documents");
			var connection = new HttpConnection(settings);
			var client = new ElasticClient(settings, connection);
			client.CreateIndex("documents", c => c
				.AddMapping<Document>(m => m.MapFromAttributes()));
			return client;
		}
	}

	public class Document
	{
		public Guid Id { get; private set; }

		[ElasticProperty(Type = FieldType.Attachment, TermVector = TermVectorOption.WithPositionsOffsets, Store = true)]
		public Attachment File { get; set; }

	    public Document()
	    {
	        Id = Guid.NewGuid();
	    }
	}

	public class Attachment
	{
		public Guid Id { get; private set; }

	    public Attachment()
	    {
	        Id = Guid.NewGuid();
	    }

		[ElasticProperty(Name = "_content")]
		public byte[] Content { get; set; }

		[ElasticProperty(Name = "_content_type")]
		public string ContentType { get; set; }

		[ElasticProperty(Name = "_name")]
		public string Name { get; set; }

		public static Attachment CreateFromFile(string path)
		{
			return new Attachment
			{
				Content = File.ReadAllBytes(path),
				ContentType = "image/png",
				Name = Path.GetFileName(path)
			};
		}

		public void SaveToFile(string path)
		{
			File.WriteAllBytes(path, Content);
		}
	}
}