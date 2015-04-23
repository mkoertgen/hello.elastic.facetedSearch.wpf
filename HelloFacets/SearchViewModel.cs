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
        private string _searchTerm = String.Empty;
        private readonly BindableCollection<DocumentViewModel> _documents = new BindableCollection<DocumentViewModel>();
        private DocumentViewModel _selectedDocument;

        public SearchViewModel(IElasticClient search, AggregationsViewModel aggregations)
        {
            if (search == null) throw new ArgumentNullException("search");
            if (aggregations == null) throw new ArgumentNullException("aggregations");
            _search = search;
            Aggregations = aggregations;
            Aggregations.CheckedChanged += (sender, args) => DoSearch();

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = "Hello Content Facets";
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

        public IEnumerable<DocumentViewModel> Documents { get { return _documents; } }

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

        public AggregationsViewModel Aggregations { get; private set; }

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
                .QueryString(_searchTerm)
                //.Query(q => q.FuzzyLikeThis(s => s.LikeText(_searchTerm)))
                .Aggregations(AggregationsSelector)
                .Highlight(h => h
                    //.FragmentSize(150).NumberOfFragments(3) // some default used in many ELS examples, defaults are (100) and (5)
                    // OnAll <=> "_all" only works if "store=true", cf.: http://www.elasticsearch.org/guide/en/elasticsearch/reference/current/search-request-highlighting.html
                    .OnFields(
                        f => f.OnField(d => d.Title),
                        f => f.OnField(d => d.Type),
                        f => f.OnField(d => d.Content))
                    // TextBlock supports inline content, cf.: http://www.wpf-tutorial.com/basic-controls/the-textblock-control-inline-formatting/
                    .PreTags("<Bold>").PostTags("</Bold>")
                    );

            return AddAggregationFilter(newSearchDescriptor);
        }

        private static AggregationDescriptor<Document> AggregationsSelector(AggregationDescriptor<Document> aggregationDescriptor = null)
        {
            var newAggregationDescriptor = (aggregationDescriptor ?? new AggregationDescriptor<Document>())
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
            if (Aggregations == null || Aggregations.Items == null) return sd;
            var typeFilter = new List<FilterContainer>();

            // filter by type
            var typeVm = Aggregations.Items.FindFirst(itm => itm.Name == "Type");
            if (typeVm != null)
            {
                var typeViewModels = typeVm
                    .Items.FindAll(i => i.IsChecked == true)
                    .Select(item => item.Aggregation).OfType<KeyItem>();
                var terms = typeViewModels.Select(t => t.Key).ToArray();
                if (terms.Any()) typeFilter.Add(new FilterDescriptor<Document>().Or(f => f.Terms(r => r.Type, terms)));
            }

            // filter by modified/changed
            typeVm = Aggregations.Items.FindFirst(itm => itm.Name == "Modified");
            if (typeVm != null)
            {
                var rtvm = typeVm.Items.FindAll(i => i.IsChecked == true)
                    .Select(item => item.Aggregation).OfType<Bucket>()
                    .SelectMany(bucket => bucket.Items).OfType<RangeItem>()
                    .Select(range => new FilterDescriptor<Document>()
                        .Range(descriptor => descriptor
                            .OnField(r => r.Changed)
                            .Greater(range.From)
                            .Lower(range.To)))
                    .ToArray();
                if (rtvm.Any()) typeFilter.Add(new FilterDescriptor<Document>().Or(rtvm));
            }

            if (!typeFilter.Any()) return sd;
            return sd.Filter(descriptor => descriptor.And(typeFilter.ToArray()));
        }
    }
}