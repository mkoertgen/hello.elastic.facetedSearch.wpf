using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using HelloFacets.MiniMods;
using Nest;

namespace HelloFacets
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SearchViewModel : Screen
    {
        private readonly IElasticClient _search;
        private string _searchTerm = string.Empty;
        private readonly BindableCollection<DocumentViewModel> _documents = new BindableCollection<DocumentViewModel>();
        private DocumentViewModel _selectedDocument;

        public SearchViewModel(IElasticClient search, AggregationsViewModel aggregations)
        {
            if (search == null) throw new ArgumentNullException(nameof(search));
            if (aggregations == null) throw new ArgumentNullException(nameof(aggregations));
            _search = search;
            Aggregations = aggregations;
            Aggregations.CheckedChanged += (sender, args) => DoSearch();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = "Hello Faceted Search";
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (value == _searchTerm) return;
                _searchTerm = value;
                NotifyOfPropertyChange();
            }
        }

        public IEnumerable<DocumentViewModel> Documents => _documents;

        public DocumentViewModel SelectedDocument
        {
            get { return _selectedDocument; }
            set
            {
                if (Equals(value, _selectedDocument)) return;
                _selectedDocument = value;
                NotifyOfPropertyChange();
            }
        }

        public AggregationsViewModel Aggregations { get; }

        public async void DoSearch()
        {
            var searchDescriptor = GetSearchDescriptor();
            var results = await _search.SearchAsync<Document>(searchDescriptor);

            var viewModels = results.HitsMetaData.Hits.Select(h => h.ToViewModel());

            _documents.Clear();
            _documents.AddRange(viewModels);
            SelectedDocument = _documents.FirstOrDefault();

            var aggregations = results.Aggregations.ToViewModel();
            Aggregations.Items = Aggregations.Items.Merge(aggregations);
        }

        private SearchDescriptor<Document> GetSearchDescriptor(SearchDescriptor<Document> searchDescriptor = null)
        {
            var newSearchDescriptor = (searchDescriptor ?? new SearchDescriptor<Document>())
                .Query(q => q.QueryString(qd => qd.Query(_searchTerm)))
                //.Query(q => q.FuzzyLikeThis(s => s.LikeText(_searchTerm)))
                .Aggregations(AggregationsSelector)
                .Highlight(h => h
                    //.FragmentSize(150).NumberOfFragments(3) // some default used in many ELS examples, defaults are (100) and (5)
                    // OnAll <=> "_all" only works if "store=true", cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/search-request-highlighting.html
                    .Fields(
                        f => f.Field(d => d.Title),
                        f => f.Field(d => d.Type),
                        f => f.Field(d => d.Content))
                    // TextBlock supports inline content, cf.: http://www.wpf-tutorial.com/basic-controls/the-textblock-control-inline-formatting/
                    .PreTags("<Bold>").PostTags("</Bold>")
                    );

            return AddAggregationFilter(newSearchDescriptor);
        }

        private static AggregationContainerDescriptor<Document> AggregationsSelector(AggregationContainerDescriptor<Document> aggregationDescriptor = null)
        {
            var newAggregationDescriptor = (aggregationDescriptor ?? new AggregationContainerDescriptor<Document>())
                // ordering terms, "_term", "_count", ...
                // default is "descending by count"
                // cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/search-aggregations-bucket-terms-aggregation.html#search-aggregations-bucket-terms-aggregation-order
                .Terms("Type", t => t.Field(document => document.Type)) //.OrderDescending("_count"))
                .DateRange("Modified", c => c
                    .Field(f => f.Changed).Ranges(dummy => dummy.To("now+1d")) // dummy range for ES parser
                    .Aggregations(d => d
                        .DateRange("Today", t => t.Field(f => f.Changed)//.Field(f => f.Changed)
                            .Ranges(r => r.From("now-1d").To("now"))) // ES uses local time!
                        .DateRange("Yesterday", t => t.Field(f => f.Changed)//.Field(f => f.Changed)
                            .Ranges(r => r.From("now-2d").To("now-1d")))
                        .DateRange("Older", t => t.Field(f => f.Changed)//.Field(f => f.Changed)
                            .Ranges(r => r.To("now-2d")))
                    ));
            return newAggregationDescriptor;
        }

        private SearchDescriptor<Document> AddAggregationFilter(SearchDescriptor<Document> sd)
        {
            if (Aggregations?.Items == null) return sd;
            var typeFilter = new List<QueryContainer>();

            // filter by type
            var typeVm = Aggregations.Items.FindFirst(itm => itm.Name == "Type");
            if (typeVm != null)
            {
                var typeAggs = typeVm
                    .Items.FindAll(i => i.IsChecked == true)
                    .Select(item => item.Aggregation).ToList();

                var typeKeys = typeAggs.OfType<KeyedValueAggregate>().SelectMany(t => t.Keys);
                var typeTerms = typeAggs.OfType<TermsAggregate>().SelectMany(t => t.Buckets.Select(k => k.Key));

                var terms = typeKeys.Concat(typeTerms).ToList();

                if (terms.Any())
                {
                    var tq = new QueryContainerDescriptor<Document>()
                        .Terms(td => td.Field(d => d.Type).Terms(terms));
                    var or = new QueryContainerDescriptor<Document>()
                        .Bool(b => b.Should(tq));
                    typeFilter.Add(or);
                }
            }

            // filter by modified/changed
            typeVm = Aggregations.Items.FindFirst(itm => itm.Name == "Modified");
            if (typeVm != null)
            {
                var rtvm = typeVm.Items.FindAll(i => i.IsChecked == true)
                    .Select(item => item.Aggregation).OfType<BucketAggregate>()
                    .SelectMany(bucket => bucket.Items).OfType<RangeBucket>()
                    .Select(range => new QueryContainerDescriptor<Document>()
                        .DateRange(descriptor => descriptor
                            .Field(r => r.Changed)
                            .GreaterThan(range.FromAsString)
                            .LessThan(range.ToAsString)))
                    .ToArray();

                if (rtvm.Any())
                {
                    var or = new QueryContainerDescriptor<Document>()
                        .Bool(b => b.Should(rtvm));
                    typeFilter.Add(or);
                }
            }

            if (!typeFilter.Any()) return sd;

            return sd.Query(d => d.Bool(b => b.Must(typeFilter.ToArray())));
        }
    }
}