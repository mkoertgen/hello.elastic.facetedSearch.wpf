using System;
using System.Collections.Generic;
using System.Linq;
using HelloFacets.MiniMods;
using Nest;

namespace HelloFacets
{
    static class ViewModelExtensions
    {
        public static IEnumerable<AggregationViewModel> ToViewModel(this IEnumerable<KeyValuePair<string, IAggregation>> aggregations)
        {
            if (aggregations == null) return Enumerable.Empty<AggregationViewModel>();
            return aggregations.Select(kvp => kvp.Value.ToViewModel(kvp.Key));
        }

        public static DocumentViewModel ToViewModel(this IHit<Document> hit)
        {
            return new DocumentViewModel(hit.Source) { Highlight = hit.Highlights.ToXaml() };
        }

        public static string ToXaml(this IEnumerable<KeyValuePair<string, Highlight>> highlights, string separator = "<LineBreak/>")
        {
            return String.Join(separator, highlights.Select(h => h.FormatHighlight(separator)));
        }

        private static string FormatHighlight(this KeyValuePair<string, Highlight> kvp, string separator = "<LineBreak/>")
        {
            return String.Join(separator, String.Format("{0}: {1}", kvp.Key, String.Join("...", kvp.Value.Highlights)));
        }



        public static IEnumerable<AggregationViewModel> Merge(this IEnumerable<AggregationViewModel> items, IEnumerable<AggregationViewModel> aggregations)
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
                // lets see if this aggregation category already exists
                var found = mergedAggregations.FirstOrDefault(itm => itm.Name == aggregation.Name);
                if (found == null)
                {
                    //mergedAggregations.Add(aggregation);
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

        private static AggregationViewModel ToViewModel(this IAggregation aggregation, string name = null)
        {
            return new AggregationViewModel
            {
                Aggregation = aggregation,
                Name = name ?? aggregation.GetName(),
                DocCount = aggregation.GetDocCount(),
                Items = aggregation.GetItems(),
            };
        }

        private static string GetName(this IAggregation aggregation)
        {
            var histogramItem = aggregation as HistogramItem;
            if (histogramItem != null)
                return histogramItem.KeyAsString;

            var keyItem = aggregation as KeyItem;
            if (keyItem != null)
                return keyItem.Key;

            var rangeItem = aggregation as RangeItem;
            if (rangeItem != null)
                return rangeItem.Key;

            var significantTermItem = aggregation as SignificantTermItem;
            if (significantTermItem != null)
                return significantTermItem.Key;

            return aggregation.ToString();
        }

        private static long GetDocCount(this IAggregation aggregation)
        {
            // Bucket
            var bucket = aggregation as Bucket;
            if (bucket != null)
                return bucket.Items.GetDocCount();
            var bucketWithCount = aggregation as IBucketWithCountAggregation;

            if (bucketWithCount != null)
                return bucketWithCount.DocCount;
            var bucketWithDocCount = aggregation as BucketWithDocCount;
            if (bucketWithDocCount != null)
                return bucketWithDocCount.DocCount;

            var histogramItem = aggregation as HistogramItem;
            if (histogramItem != null)
                return histogramItem.DocCount;

            var keyItem = aggregation as KeyItem;
            if (keyItem != null)
                return keyItem.DocCount;

            var rangeItem = aggregation as RangeItem;
            if (rangeItem != null)
                return rangeItem.DocCount;

            var significantTermItem = aggregation as SignificantTermItem;
            if (significantTermItem != null)
                return significantTermItem.DocCount;

            var singleBucket = aggregation as SingleBucket;
            if (singleBucket != null)
                return singleBucket.DocCount;

            return 0L;
        }

        private static long GetDocCount(this IEnumerable<IAggregation> aggregations)
        {
            return aggregations != null ? aggregations.Aggregate(0L, (sum, a) => sum + a.GetDocCount()) : 0L;
        }

        private static IEnumerable<AggregationViewModel> GetItems(this IAggregation aggregation)
        {
            if (aggregation == null) return Enumerable.Empty<AggregationViewModel>();

            var items = new List<AggregationViewModel>();

            var bucket = aggregation as Bucket;
            if (bucket != null)
                items.AddRange(bucket.Items.ToViewModel());

            var bucketWithDocCount = aggregation as BucketWithDocCount;
            if (bucketWithDocCount != null)
                items.AddRange(bucketWithDocCount.Items.ToViewModel());

            var bucketAggregation = aggregation as IBucketAggregation;
            if (bucketAggregation != null)
                items.AddRange(bucketAggregation.Aggregations.ToViewModel());

            return items;
        }

        private static IEnumerable<AggregationViewModel> ToViewModel(this IEnumerable<IAggregation> aggregations)
        {
            if (aggregations == null) return Enumerable.Empty<AggregationViewModel>();

            // NOTE: filter out range items. 
            // Convention: range items are wrapped in a named aggregation so a nice display name can be used.
            //return aggregations.Select(ToViewModel);

            // NOTE: Use loop to preserve order (instead of set operations)
            var items = new List<AggregationViewModel>();
            foreach (var aggregation in aggregations)
            {
                var rangeItem = aggregation as RangeItem;
                if (rangeItem != null)
                    items.AddRange(rangeItem.Aggregations.ToViewModel());
                else
                    items.Add(aggregation.ToViewModel());
            }
            return items;
        }
    }
}