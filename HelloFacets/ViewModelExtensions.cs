using System;
using System.Collections.Generic;
using System.Linq;
using HelloFacets.MiniMods;
using Nest;

namespace HelloFacets
{
    internal static class ViewModelExtensions
    {
        public static IEnumerable<AggregationViewModel> ToViewModel(
            this IDictionary<string, IAggregate> aggregations)
        {
            return aggregations?.Select(kvp => kvp.Value.ToViewModel(kvp.Key))
                   ?? Enumerable.Empty<AggregationViewModel>();
        }

        public static DocumentViewModel ToViewModel(this IHit<Document> hit)
        {
            return new DocumentViewModel(hit.Source) {Highlight = hit.Highlights.ToXaml()};
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
                    continue;

                // if yes we need to compare each separate item
                foreach (var concreteAggregation in aggregation.Items)
                {
                    if (string.IsNullOrWhiteSpace(concreteAggregation.Name))
                        continue;

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

        private static AggregationViewModel ToViewModel(this IAggregate aggregate, string name)
        {
            return new AggregationViewModel
            {
                Aggregation = aggregate,
                Name = name,
                DocCount = aggregate.GetDocCount(),
                Items = aggregate.GetItems()
            };
        }

        private static string ToXaml(this HighlightFieldDictionary highlights, string separator = "<LineBreak/>")
        {
            return string.Join(separator, highlights.Select(h => h.FormatHighlight(separator)));
        }

        private static string FormatHighlight(this KeyValuePair<string, HighlightHit> kvp,
            string separator = "<LineBreak/>")
        {
            return string.Join(separator, $"{kvp.Key}: {string.Join("...", kvp.Value.Highlights)}");
        }

        internal static long GetDocCount(this IAggregate aggregate)
        {
            // buckets
            var bucketAgg = aggregate as BucketAggregate;
            if (bucketAgg != null)
                return bucketAgg.GetDocCount();

            var singleBucketAgg = aggregate as SingleBucketAggregate;
            if (singleBucketAgg != null)
                return singleBucketAgg.GetDocCount();

            // multi bucket aggregates
            var sigTermsAgg = aggregate as SignificantTermsAggregate;
            if (sigTermsAgg != null)
                return sigTermsAgg.GetDocCount();

            var termsAgg = aggregate as TermsAggregate;
            if (termsAgg != null)
                return termsAgg.GetDocCount();

            var filtersAgg = aggregate as FiltersAggregate;
            if (filtersAgg != null)
                return filtersAgg.GetDocCount();

            // fallback (default)
            var bucketsAgg = aggregate as AggregationsHelper;
            if (bucketsAgg != null)
                return bucketsAgg.Aggregations.GetDocCount();

            // stats
            var topHistAgg = aggregate as TopHitsAggregate;
            if (topHistAgg != null)
                return topHistAgg.Total;

            var statsAgg = aggregate as StatsAggregate;
            if (statsAgg != null)
                return statsAgg.Count;

            var exStatsAgg = aggregate as ExtendedStatsAggregate;
            if (exStatsAgg != null)
                return exStatsAgg.Count;


            return 0L;
        }

        private static long GetDocCount(this BucketAggregate bucketAggregate)
        {
            if (bucketAggregate == null) throw new ArgumentNullException(nameof(bucketAggregate));
            return bucketAggregate.DocCount != 0
                ? bucketAggregate.DocCount
                : bucketAggregate.Items.GetDocCount();
        }

        private static long GetDocCount(this SingleBucketAggregate bucketAggregate)
        {
            if (bucketAggregate == null) throw new ArgumentNullException(nameof(bucketAggregate));
            return bucketAggregate.DocCount != 0
                ? bucketAggregate.DocCount
                : bucketAggregate.Aggregations.GetDocCount();
        }


        private static long GetDocCount<TBucket>(this MultiBucketAggregate<TBucket> multiBucketAggregate)
            where TBucket : IBucket
        {
            if (multiBucketAggregate == null) throw new ArgumentNullException(nameof(multiBucketAggregate));
            if (multiBucketAggregate.Aggregations != null && multiBucketAggregate.Aggregations.Any())
                return multiBucketAggregate.Aggregations.GetDocCount();
            if (multiBucketAggregate.Buckets != null && multiBucketAggregate.Buckets.Any())
                return multiBucketAggregate.Buckets.GetDocCount();
            return 0L;
        }

        private static long GetDocCount(this IDictionary<string, IAggregate> aggregations)
        {
            return aggregations?.Values.Aggregate(0L, (sum, a) => sum + a.GetDocCount()) ?? 0L;
        }

        private static long GetDocCount<TBucket>(this IEnumerable<TBucket> buckets) where TBucket : IBucket
        {
            return buckets?.Aggregate(0L, (sum, b) => sum + b.GetDocCount()) ?? 0L;
        }

        internal static long GetDocCount(this IBucket bucket)
        {
            var kb = bucket as KeyedBucket;
            if (kb != null)
                return BucketDocCount(kb, kb.DocCount.GetValueOrDefault());

            var rb = bucket as RangeBucket;
            if (rb != null)
                return BucketDocCount(rb, rb.DocCount);

            var hb = bucket as HistogramBucket;
            if (hb != null)
                return BucketDocCount(hb, hb.DocCount);

            var stb = bucket as SignificantTermsBucket;
            if (stb != null)
                return BucketDocCount(stb, stb.DocCount);

            var fbi = bucket as FiltersBucketItem;
            if (fbi != null)
                return BucketDocCount(fbi, fbi.DocCount);

            return 0L;
        }

        private static long BucketDocCount(BucketBase bucketBase, long docCount)
        {
            if (bucketBase == null) throw new ArgumentNullException(nameof(bucketBase));
            return docCount > 0 ? docCount : bucketBase.Aggregations.GetDocCount();
        }


        private static IEnumerable<AggregationViewModel> GetItems(this IAggregate aggregate)
        {
            if (aggregate == null) return Enumerable.Empty<AggregationViewModel>();

            var aggs = aggregate as BucketAggregateBase;
            if (aggs != null)
                return aggs.Aggregations.ToViewModel();

            var bucketAgg = aggregate as BucketAggregate;
            if (bucketAgg != null)
                return bucketAgg.Items.ToViewModel();

            return Enumerable.Empty<AggregationViewModel>();
        }

        private static IEnumerable<AggregationViewModel> ToViewModel(this IEnumerable<IBucket> buckets)
        {
            if (buckets == null) return Enumerable.Empty<AggregationViewModel>();
            var items = new List<AggregationViewModel>();

            foreach (var bucket in buckets)
            {
                var kb = bucket as KeyedBucket;
                if (kb != null)
                    items.Add(kb.ToViewModel());

                var bucketBase = bucket as BucketBase;
                if (bucketBase != null)
                    items.AddRange(bucketBase.Aggregations.ToViewModel());
            }

            return items;
        }

        private static AggregationViewModel ToViewModel(this KeyedBucket kb)
        {
            return new AggregationViewModel
            {
                Name = kb.Key,
                DocCount = kb.GetDocCount(),
                Items = kb.Aggregations.ToViewModel()
            };
        }
    }
}