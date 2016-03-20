using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using FluentAssertions;
using Nest;

namespace HelloAggregations.Tests
{
    internal class TestFactory
    {
        public static IElasticClient CreateClient(bool purge = false)
        {
            var node = new Uri("http://localhost:9200");

            var pool = new SingleNodeConnectionPool(node);
            var settings = new ConnectionSettings(pool)
                .DefaultIndex("sports")
                .PrettyJson();

            var client = CreateClient(settings);

            if (purge)
                IndexTestData(client);

            return client;
        }

        private static IElasticClient CreateClient(IConnectionSettingsValues settings)
        {
            var client = new ElasticClient(settings);
            client.RootNodeInfo().IsValid.Should().BeTrue();
            return client;
        }

        private static void IndexTestData(IElasticClient client)
        {
            if (client.IndexExists("sports").Exists)
                client.DeleteIndex("sports");

            client.CreateIndex("sports", s => s
                // add mapping for location to geo_point, cf.: http://markswanderingthoughts.nl/post/84327066530/geo-distance-searching-in-elasticserach-with-nest
                .Mappings(map => map
                    .Map<Athlete>(m => m
                        .AutoMap()
                        .Properties(p => p
                            .GeoPoint(g => g.Name(n => n.Location).LatLon()))
                    )
                )
            );

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