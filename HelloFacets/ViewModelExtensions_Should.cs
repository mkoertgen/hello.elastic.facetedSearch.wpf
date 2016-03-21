using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Nest;
using NUnit.Framework;

namespace HelloFacets
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    internal class ViewModelExtensions_Should
    {
        #region Merge

        [Test]
        public void Add_new_aggregations_if_initially_empty()
        {
            var items = Enumerable.Empty<AggregationViewModel>();

            var expected = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 3,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 2}
                        , new AggregationViewModel {Name = "draft", DocCount = 1}
                    }
                }
            };

            var actual = items.Merge(expected).ToList();

            VerifyEquals(actual.ToList(), expected.ToList());
        }

        [Test]
        public void Update_DocCounts_on_equal_items()
        {
            var items = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 3,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 2}
                        , new AggregationViewModel {Name = "draft", DocCount = 1}
                    }
                }
            };

            var expected = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 6,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 4}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                    }
                }
            };

            var actual = items.Merge(expected).ToList();

            VerifyEquals(expected, actual.ToList());
        }


        [Test]
        public void Add_new_aggregations()
        {
            var items = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 3,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 2}
                        , new AggregationViewModel {Name = "draft", DocCount = 1}
                    }
                }
            };

            var expected = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 12,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 8}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                        , new AggregationViewModel {Name = "unread", DocCount = 2}
                    }
                }
            };

            var actual = items.Merge(expected).ToList();

            VerifyEquals(expected, actual.ToList());
        }

        [Test]
        public void Keep_removed_aggregations_with_DocCount_0()
        {
            var items = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 5,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 2}
                        , new AggregationViewModel {Name = "draft", DocCount = 1}
                        , new AggregationViewModel {Name = "unread", DocCount = 2}
                    }
                }
            };

            var toMerge = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 10,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 8}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                    }
                }
            };

            var expected = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 10,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 8}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                        , new AggregationViewModel {Name = "unread", DocCount = 0}
                    }
                }
            };

            var actual = items.Merge(toMerge).ToList();

            VerifyEquals(expected, actual.ToList());
        }

        [Test]
        public void Skip_Empty_named_Terms_aggregations()
        {
            var items = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 2,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 2}
                    }
                }
            };

            var toMerge = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 12,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 8}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                        , new AggregationViewModel {Name = "", DocCount = 2}
                    }
                }
            };

            var expected = new[]
            {
                new AggregationViewModel
                {
                    Name = "Status",
                    DocCount = 10,
                    Items = new[]
                    {
                        new AggregationViewModel {Name = "new", DocCount = 8}
                        , new AggregationViewModel {Name = "draft", DocCount = 2}
                    }
                }
            };


            var actual = items.Merge(toMerge).ToList();

            VerifyEquals(expected, actual.ToList());

        }

        private static void VerifyEquals(IList<AggregationViewModel> actual, IList<AggregationViewModel> expected)
        {
            actual.Count.Should().Be(expected.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                var a = actual[i];
                var b = expected[i];
                VerifyEquals(a, b);
                VerifyEquals(a.Items.ToList(), b.Items.ToList());
            }
        }

        private static void VerifyEquals(AggregationViewModel actual, AggregationViewModel expected)
        {
            if (actual == null)
            {
                if (expected == null) return;
                throw new AssertionException("Expected \"expected\" to be null");
            }

            actual.DocCount.Should().Be(expected.DocCount);
            actual.Name.Should().Be(expected.Name);

            // ReSharper disable once TailRecursiveCall
            VerifyEquals(actual.Parent, expected.Parent);
        }

        #endregion

        #region DocCount

        [Test]
        public void Get_Aggregate_DocCount()
        {
            IAggregate aggregate = new BucketAggregate {DocCount = 4};
            aggregate.GetDocCount().Should().Be(4);

            aggregate = new BucketAggregate
            {
                DocCount = 0,
                Items = new IBucket[]
                {
                    new HistogramBucket {DocCount = 2},
                    new KeyedBucket(),
                    new FiltersBucketItem {DocCount = 2}
                }
            };
            aggregate.GetDocCount().Should().Be(4);

            aggregate = new ExtendedStatsAggregate {Count = 4};
            aggregate.GetDocCount().Should().Be(4);

            // MultiBucketAggregate<>
            aggregate = new FiltersAggregate(new Dictionary<string, IAggregate>
            {
                {"geo_0", new GeoBoundsAggregate()},
                {"keys_0", new KeyedValueAggregate {Keys = new[] {"key1", "key2"}}},
                {"terms_3", new SignificantTermsAggregate
                    {
                        DocCount = 0,
                        Buckets = new List<SignificantTermsBucket>
                        {
                            new SignificantTermsBucket { Key = "a", DocCount = 1},
                            new SignificantTermsBucket { Key = "b", DocCount = 2}
                        }
                    }},
                {"stats_1", new StatsAggregate { Count = 1} },
                {"topHits_4", new TopHitsAggregate { Total = 4} },
                {"value_0", new ValueAggregate() }
            });
            aggregate.GetDocCount().Should().Be(8);

            // BucketAggregateBase
            aggregate = new SingleBucketAggregate(new Dictionary<string, IAggregate>
            {
                {"terms_3", new TermsAggregate
                    {
                        Buckets = new List<KeyedBucket>
                        {
                            new KeyedBucket { Key = "a", DocCount = 1},
                            new KeyedBucket { Key = "b", DocCount = 2}
                        }
                    }},
            });
            aggregate.GetDocCount().Should().Be(3);


        }

        [Test]
        public void Get_Bucket_DocCount()
        {
            var aggs = new Dictionary<string, IAggregate>
            {
                {"foo", new StatsAggregate { Count = 4 }}
            };

            IBucket bucket = new HistogramBucket {DocCount = 4};
            bucket.GetDocCount().Should().Be(4);
            bucket = new HistogramBucket(aggs);
            bucket.GetDocCount().Should().Be(4);

            bucket = new KeyedBucket();
            bucket.GetDocCount().Should().Be(0);
            bucket = new KeyedBucket {DocCount = 4};
            bucket.GetDocCount().Should().Be(4);
            bucket = new KeyedBucket(aggs);
            bucket.GetDocCount().Should().Be(4);

            bucket = new RangeBucket {DocCount = 4};
            bucket.GetDocCount().Should().Be(4);
            bucket = new RangeBucket(aggs);
            bucket.GetDocCount().Should().Be(4);

            bucket = new SignificantTermsBucket {DocCount = 4};
            bucket.GetDocCount().Should().Be(4);
            bucket = new SignificantTermsBucket(aggs);
            bucket.GetDocCount().Should().Be(4);

            bucket = new FiltersBucketItem { DocCount = 4 };
            bucket.GetDocCount().Should().Be(4);
            bucket = new FiltersBucketItem(aggs);
            bucket.GetDocCount().Should().Be(4);
        }

        #endregion
    }
}