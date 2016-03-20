using System.Collections.Generic;
using System.Linq;
using HelloFacets.MiniMods;
using Nest;

namespace HelloFacets
{
    internal static class ViewModelExtensions
    {
        public static IEnumerable<AggregationViewModel> ToViewModel(this IEnumerable<KeyValuePair<string, IAggregate>> aggregations)
        {
            if (aggregations == null) return Enumerable.Empty<AggregationViewModel>();
            return aggregations.Select(kvp => kvp.Value.ToViewModel(kvp.Key));
        }

        public static DocumentViewModel ToViewModel(this IHit<Document> hit)
        {
            return new DocumentViewModel(hit.Source) { Highlight = hit.Highlights.ToXaml() };
        }

        public static string ToXaml(this HighlightFieldDictionary highlights, string separator = "<LineBreak/>")
        {
            return string.Join(separator, highlights.Select(h => h.FormatHighlight(separator)));
        }

        private static string FormatHighlight(this KeyValuePair<string, HighlightHit> kvp, string separator = "<LineBreak/>")
        {
            return string.Join(separator, $"{kvp.Key}: {string.Join("...", kvp.Value.Highlights)}");
        }


        public static IEnumerable<AggregationViewModel> Merge(this IEnumerable<AggregationViewModel> items, 
            IEnumerable<AggregationViewModel> aggregations)
        {
            // first time initialization -> plain copy
            var mergedAggregations = items.ToList();

            if (!mergedAggregations.Any())
                return aggregations;

            // first, zero doc count
            mergedAggregations.Apply(item => item.DocCount = 0);

            // see if we need to add some facets...
            foreach (var aggregation in aggregations)
            {
                // lets see if this aggregate category already exists
                var found = mergedAggregations.FirstOrDefault(itm => itm.Name == aggregation.Name);
                if (found == null)
                {
                    //mergedAggregations.Add(aggregate);
                    continue;
                }

                // if yes we need to comare each separate item
                foreach (var concreteAggregation in aggregation.Items)
                {
                    var concreteFound = found.Items.FirstOrDefault(itm => itm.Name == concreteAggregation.Name);

                    // if not found again, just add an continue
                    if (concreteFound == null)
                    {
                        found.Add(concreteAggregation);
                        continue;
                    }

                    // otherwise update
                    concreteFound.Aggregation = concreteAggregation.Aggregation;
                    concreteFound.DocCount = concreteAggregation.DocCount;
                }
            }

            // update/sum DocCounts
            mergedAggregations.ForEach(item => item.DocCount = item.Items.Sum(i => i.DocCount));

            return mergedAggregations;
        }

        private static AggregationViewModel ToViewModel(this IAggregate aggregate, string name = null)
        {
            return new AggregationViewModel
            {
                Aggregation = aggregate,
                Name = name ?? "todo",
                DocCount = aggregate.GetDocCount(),
                Items = aggregate.GetItems(),
            };
        }

        private static long GetDocCount(this IAggregate aggregate)
        {
            // Bucket
            var bucketAgg = aggregate as BucketAggregate;
            if (bucketAgg != null)
                return bucketAgg.DocCount;

            var singleBucketAgg = aggregate as SingleBucketAggregate;
            if (singleBucketAgg != null)
                return singleBucketAgg.DocCount;

            var significantTermsAgg = aggregate as SignificantTermsAggregate;
            if (significantTermsAgg != null)
                return significantTermsAgg.DocCount;

            var bucketsAgg = aggregate as BucketAggregateBase;
            if (bucketsAgg != null)
                return bucketsAgg.Aggregations.Values.GetDocCount();

            //var histogramBucket = aggregate as HistogramBucket;
            //if (histogramBucket != null)
            //    return histogramBucket.DocCount;

            //var keyedBucket = aggregate as KeyedBucket;
            //if (keyedBucket != null)
            //    return keyedBucket.DocCount.GetValueOrDefault();

            //var rangeBucket = aggregate as RangeBucket;
            //if (rangeBucket != null)
            //    return rangeBucket.DocCount;

            //var significantTermsBucket = aggregate as SignificantTermsBucket;
            //if (significantTermsBucket != null)
            //    return significantTermsBucket.DocCount;

            return 0L;
        }

        private static long GetDocCount(this IEnumerable<IAggregate> aggregations)
        {
            return aggregations?.Aggregate(0L, (sum, a) => sum + a.GetDocCount()) ?? 0L;
        }

        private static IEnumerable<AggregationViewModel> GetItems(this IAggregate aggregate)
        {
            if (aggregate == null) return Enumerable.Empty<AggregationViewModel>();

            var items = new List<AggregationViewModel>();

            var bucketAgg = aggregate as BucketAggregateBase;
            if (bucketAgg != null)
                items.AddRange(bucketAgg.Aggregations.ToViewModel());

            //var bucketWithDocCount = aggregate as BucketWithDocCount;
            //if (bucketWithDocCount != null)
            //    items.AddRange(bucketWithDocCount.Items.ToViewModel());

            //var bucketAggregation = aggregate as IBucketAggregation;
            //if (bucketAggregation != null)
            //    items.AddRange(bucketAggregation.Aggregations.ToViewModel());

            return items;
        }

        private static IEnumerable<AggregationViewModel> ToViewModel(this IEnumerable<IAggregate> aggregations)
        {
            if (aggregations == null) return Enumerable.Empty<AggregationViewModel>();

            // NOTE: filter out range items. 
            // Convention: range items are wrapped in a named aggregate so a nice display name can be used.
            //return aggregations.Select(ToViewModel);

            // NOTE: Use loop to preserve order (instead of set operations)
            var items = new List<AggregationViewModel>();
            foreach (var aggregate in aggregations)
            {
                var bucketAgg = aggregate as BucketAggregateBase;
                if (bucketAgg != null)
                    items.AddRange(bucketAgg.Aggregations.ToViewModel());
                else
                    items.Add(aggregate.ToViewModel());
            }
            return items;
        }
    }
}