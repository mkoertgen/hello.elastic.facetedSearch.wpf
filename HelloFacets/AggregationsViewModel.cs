using System;
using System.Collections.Generic;
using System.ComponentModel;
using HelloFacets.MiniMods;

namespace HelloFacets
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AggregationsViewModel : TreeViewModel<AggregationViewModel>
    {
        private readonly List<ITreeItemViewModel> _items  = new List<ITreeItemViewModel>();
    
        public event EventHandler CheckedChanged = delegate { };

        public AggregationsViewModel()
        {
            CacheItems();
            HookItemsChanged();
        }

        protected override void OnItemsChanged()
        {
            UnhookItemsChanged();

            base.OnItemsChanged();

            CacheItems();
            HookItemsChanged();
        }

        private void HookItemsChanged()
        {
            _items.Apply(item => item.PropertyChanged += ItemPropertyChanged);
        }

        private void CacheItems()
        {
            _items.Clear();
            _items.AddRange(this.Items);
        }

        private void UnhookItemsChanged()
        {
            _items.Apply(item => item.PropertyChanged -= ItemPropertyChanged);
        }

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
                OnCheckedChanged();
        }

        private void OnCheckedChanged()
        {
            CheckedChanged(this, EventArgs.Empty);
        }
    }
}