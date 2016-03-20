using System;
using System.Collections.Generic;
using Elasticsearch.Net.Connection;
using FluentAssertions;
using Nest;

namespace HelloAggregations.Tests
{
    internal class TestFactory
    {
        public static IElasticClient CreateClient(bool purge = false)
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .SetDefaultIndex("sports")
                .UsePrettyResponses();

            var connection = new HttpConnection(settings); // new InMemoryConnection(settings);
            var client = CreateClient(settings, connection);

            if (purge)
            {
                SetupIndexAndMapping(client);
                IndexTestData(client);
            }

            return client;
        }

        private static IElasticClient CreateClient(IConnectionSettingsValues settings, IConnection connection)
        {
            var client = new ElasticClient(settings, connection);
            client.RootNodeInfo().IsValid.Should().BeTrue();
            return client;
        }

        private static void SetupIndexAndMapping(IElasticClient client)
        {
            if (client.IndexExists(i => i.Index("sports")).Exists)
                client.DeleteIndex(i => i.Index("sports"));

            client.CreateIndex("sports", s => s
                // add mapping for location to geo_point, cf.: http://markswanderingthoughts.nl/post/84327066530/geo-distance-searching-in-elasticserach-with-nest
                .AddMapping<Athlete>(f => f
                  .MapFromAttributes()
                  .Properties(p => p
                    .GeoPoint(g => g.Name(n => n.Location).IndexLatLon())
                  )
                )
              );
        }

        private static void IndexTestData(IElasticClient client)
        {
            client.IndexMany(Athletes);
        }

        public static readonly IList<Athlete> Athletes = new List<Athlete>
        {
            new Athlete{Name = "Michael", BirthDate = new DateTime(1989,1,10),Sport = "Baseball", Rating = new []{5,4}, 
                Location = new Location {Lat=46.22, Lon = -68.45}},
            new Athlete{Name = "Bob", BirthDate = new DateTime(1989,1,10),Sport = "Baseball", Rating = new []{3,4}, 
                Location = new Location {Lat=45.21, Lon = -68.35}},
            new Athlete{Name = "Jim", BirthDate = new DateTime(1989,2,11),Sport = "Baseball", Rating = new []{3,2}, 
                Location = new Location {Lat=45.16, Lon = -63.58}},
            new Athlete{Name = "Joe", BirthDate = new DateTime(1988,3,10),Sport = "Baseball", Rating = new []{4,3}, 
                Location = new Location {Lat=45.22, Lon = -68.53}},
            new Athlete{Name = "Tim", BirthDate = new DateTime(1992,5,20),Sport = "Baseball", Rating = new []{3,3}, 
                Location = new Location {Lat=46.22, Lon = -68.85}},
            new Athlete{Name = "Alfred", BirthDate = new DateTime(1992,2,28),Sport = "Baseball", Rating = new []{2,2}, 
                Location = new Location {Lat=45.12, Lon = -68.35}},
            new Athlete{Name = "Jeff", BirthDate = new DateTime(1990,9,9),Sport = "Baseball", Rating = new []{2,3}, 
                Location = new Location {Lat=46.12, Lon = -68.55}},
            new Athlete{Name = "Will", BirthDate = new DateTime(1990,1,4),Sport = "Baseball", Rating = new []{4,4}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Mick", BirthDate = new DateTime(1988,1,3),Sport = "Baseball", Rating = new []{3,4}, 
                Location = new Location {Lat=46.22, Lon = -68.45}},
            new Athlete{Name = "Pong", BirthDate = new DateTime(1989,1,10),Sport = "Baseball", Rating = new []{1,3}, 
                Location = new Location {Lat=45.21, Lon = -68.35}},
            new Athlete{Name = "Ray", BirthDate = new DateTime(1989,2,11),Sport = "Baseball", Rating = new []{2,2}, 
                Location = new Location {Lat=45.16, Lon = -63.58}},
            new Athlete{Name = "Ping", BirthDate = new DateTime(1988,3,10),Sport = "Baseball", Rating = new []{4,3}, 
                Location = new Location {Lat=45.22, Lon = -68.35}},
            new Athlete{Name = "Duke", BirthDate = new DateTime(1992,5,20),Sport = "Baseball", Rating = new []{5,2}, 
                Location = new Location {Lat=46.22, Lon = -68.85}},
            new Athlete{Name = "Hal", BirthDate = new DateTime(1992,2,28),Sport = "Baseball", Rating = new []{4,2}, 
                Location = new Location {Lat=45.12, Lon = -68.35}},
            new Athlete{Name = "Charge", BirthDate = new DateTime(1990,9,9),Sport = "Baseball", Rating = new []{3,2}, 
                Location = new Location {Lat=46.12, Lon = -68.55}},
            new Athlete{Name = "Barry", BirthDate = new DateTime(1990,1,4),Sport = "Baseball", Rating = new []{5,2}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Bank", BirthDate = new DateTime(1988,1,3),Sport = "Golf", Rating = new []{6,4}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Bingo", BirthDate = new DateTime(1988,1,3),Sport = "Golf", Rating = new []{10,7}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "James", BirthDate = new DateTime(1988,1,3),Sport = "Basketball", Rating = new []{10,8}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Wayne", BirthDate = new DateTime(1988,1,3),Sport = "Hockey", Rating = new []{10,10}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Brady", BirthDate = new DateTime(1988,3,1),Sport = "Football", Rating = new []{10,10}, 
                Location = new Location {Lat=46.25, Lon = -68.55}},
            new Athlete{Name = "Lewis", BirthDate = new DateTime(1988,3,1),Sport = "Football", Rating = new []{10,10}, 
                Location = new Location {Lat=46.25, Lon = -68.55}}
        };
    }
}