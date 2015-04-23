using HelloFacets.MiniMods;
using Nest;

namespace HelloFacets
{
    public class AggregationViewModel : TreeItemViewModel<AggregationViewModel>
    {
        private IAggregation _aggregation;
        private string _name;
        private long _docCount;

        public IAggregation Aggregation
        {
            get { return _aggregation; }
            set
            {
                if (Equals(value, _aggregation)) return;
                _aggregation = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public long DocCount
        {
            get { return _docCount; }
            set
            {
                if (value == _docCount) return;
                _docCount = value;
                OnPropertyChanged();
                // skip binding IsEnabled t DocCount > 0 as this prevents widening up a restricted search again.
                //IsEnabled = (_docCount > 0);
            }
        }
    }
}