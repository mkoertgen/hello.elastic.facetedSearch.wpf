using System;
using System.Globalization;
using Elasticsearch.Net.Connection;
using Nest;

namespace HelloFacets.Tests
{
    static class TestFactory
    {
        private const string IndexName = "hello";

        public static IElasticClient CreateClient(bool purge=true)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .SetDefaultIndex(IndexName)
                .UsePrettyResponses();

            var connection = new HttpConnection(settings); // new InMemoryConnection(settings);
            var client = CreateClient(settings, connection);

            SetupIndexAndMapping(client, purge);

            return client;
        }

        private static IElasticClient CreateClient(IConnectionSettingsValues settings, IConnection connection)
        {
            var client = new ElasticClient(settings, connection);
            if (!client.RootNodeInfo().IsValid)
                throw new InvalidOperationException("ElasticSearch root node is invalid. Please check your ElasticSearch Server");
            return client;
        }

        private static void SetupIndexAndMapping(IElasticClient client, bool purge = false)
        {
            if (client.IndexExists(i => i.Index(IndexName)).Exists)
            {
                if (purge)
                    client.DeleteIndex(i => i.Index(IndexName));
                else
                    return;
            }

            client.CreateIndex(IndexName, s => s
                .NumberOfReplicas(0)
                .NumberOfShards(1)
                // enable caching by default, cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/index-modules-shard-query-cache.html
                .Settings(dict => dict
                    .Add("cache.query.enable", true)
                )
                // add mapping for location to geo_point, cf.: http://markswanderingthoughts.nl/post/84327066530/geo-distance-searching-in-elasticserach-with-nest
                .AddMapping<Document>(f => f
                  .MapFromAttributes()
                  .Properties(p => p
                    .GeoPoint(g => g.Name(n => n.Location).IndexLatLon())
                  )
                  // enable default ttl, however cannot specify a field source for that
                    // cf.: https://github.com/elasticsearch/elasticsearch-net/blob/develop/src/Tests/Nest.Tests.Unit/Core/Map/FluentMappingFullExampleTests.cs
                  .TtlField(ttl => ttl.Enable().Default("10d"))
                  // enable timestamp field
                  .TimestampField(ts => ts.Enabled().Path(r => r.Changed))
                )
              );


            // bulk indexing does not allow to specify ttl per document,
            // so we index each document individually
            //client.IndexMany(TestData);

            foreach (var document in TestData)
            {
                if (document.TimeToLive.HasValue)
                {
                    // default time unit is milliseconds, cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/mapping-ttl-field.html
                    var ttl = ((long) document.TimeToLive.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);
                    client.Index(document, i => i.Ttl(ttl));
                }
                else
                {
                    client.Index(document); // uses default as specified in mapping
                }
            }
        }

        public static readonly Document[] TestData =
        {
            new Document
            {
                Type = "type1",
                Title = "Title1", 
                Created = DateTime.UtcNow, 
                Changed = DateTime.UtcNow, 
                Content = "Content1",
                Location = new Location {Lat=46.22, Lon = -68.45},
            }, 
            new Document
            {
                Type = "type2",
                Title = "Title2", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(15.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(1.5), 
                Content = "Content2",
                Location = new Location {Lat=45.21, Lon = -68.35}
            }, 
            new Document
            {
                Type = "type4",
                Title = "Title4", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(2.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(1.5), 
                Content = "Content3",
                Location = new Location {Lat=45.16, Lon = -63.58}
            },
            new Document
            {
                Type = "type4",
                Title = "Title23", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(35.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(35.5), 
                Content = "Content3",
                Location = new Location {Lat=45.16, Lon = -63.58}
            },
            new Document
            {
                Type = "type4",
                Title = "Title42", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(2.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(1.5), 
                Content = "Content3",
                Location = new Location {Lat=45.16, Lon = -63.58}
            },
            new Document
            {
                Type = "type0",
                Title = "I am old", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(102.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(101.5), 
                Content = "Content3",
                Location = new Location {Lat=45.16, Lon = -63.58}
            },
            new Document
            {
                Type = "type0",
                Title = "I am old but recently changed", 
                Created = DateTime.UtcNow - TimeSpan.FromHours(102.5), 
                Changed = DateTime.UtcNow - TimeSpan.FromHours(24.5), 
                Content = "Content3",
                Location = new Location {Lat=45.16, Lon = -63.58}
            }
        };
    }
}