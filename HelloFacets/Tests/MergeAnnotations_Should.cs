using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace HelloFacets.Tests
{
    // ReSharper disable InconsistentNaming
    [TestFixture, Description("verifies #11533")]
    class MergeAnnotations_Should
    {
        [Test]
        public void Return_new_annotations_if_empty()
        {
            var items = Enumerable.Empty<AggregationViewModel>();

            var expected = new[]
            {
                new AggregationViewModel { Name= "Status", DocCount = 3,
                    Items = new [] {
                        new AggregationViewModel { Name = "new", DocCount = 2}
                        , new AggregationViewModel { Name = "draft", DocCount = 1}
                    }
                }
            };

            var actual = items.Merge(expected).ToList();

            VerifyEquals(actual, expected);
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

            VerifyEquals(expected, actual);
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

            VerifyEquals(expected, actual);
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

            VerifyEquals(expected, actual);
        }

        private static void VerifyEquals(IList<AggregationViewModel> actual, IList<AggregationViewModel> expected)
        {
            actual.Count.Should().Be(expected.Count);
            for (int i = 0; i < expected.Count; i++)
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
            VerifyEquals(actual.Parent, expected.Parent);
        }
    }
}
