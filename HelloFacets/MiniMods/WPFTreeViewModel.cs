using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace HelloFacets.MiniMods
{
    public interface ITreeItemViewModel : INotifyPropertyChanged
    {
        bool? IsChecked { get; set; }
        bool IsEnabled { get; set; }
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        IEnumerable<ITreeItemViewModel> Items { get; set; }
        ITreeItemViewModel Parent { get; set; }
    }

    public interface ITreeViewModel : ITreeItemViewModel
    {
        ITreeItemViewModel SelectedItem { get; set; }
    }

    public interface ITreeItemViewModel<TItem> : ITreeItemViewModel 
        where TItem : ITreeItemViewModel
    {
        new TItem Parent { get; set; }
        new IEnumerable<TItem> Items { get; set; }
    }

    public interface ITreeViewModel<TItem> : ITreeItemViewModel<TItem> 
        where TItem : ITreeItemViewModel
    {
         TItem SelectedItem { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class TreeItemViewModel 
        : ITreeItemViewModel
        , IEquatable<TreeItemViewModel>
    {
        private bool _isEnabled = true;
        private bool _selected;
        private bool _expanded;
        private ITreeItemViewModel _parent;
        private readonly ObservableCollection<ITreeItemViewModel> _items = new ObservableCollection<ITreeItemViewModel>();

        private readonly Guid _id = Guid.NewGuid();
        private bool? _isChecked = false;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                if (_selected == value) return;
                _selected = value;
                OnPropertyChanged("IsSelected");

                // Expand the parent and all the way up to the root.
                if (_selected && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public bool IsExpanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded == value) return;
                _expanded = value;
                OnPropertyChanged("IsExpanded");

                // if expanded, expand all the way up to the root.
                if (_expanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public ITreeItemViewModel Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged("Parent");

                // if expanded, expand all the way up to the root.
                if ((_parent != null) && (_selected || _expanded))
                    _parent.IsExpanded = true;
            }
        }

        public IEnumerable<ITreeItemViewModel> Items
        {
            get { return _items; }
            set
            {
                _items.Clear();
                if (value != null)
                    value.ToList().ForEach(Add);
                OnPropertyChanged("Items");
                OnItemsChanged();
            }

        }

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                // the check & set is important to avoid infinite recursion (stack overflow)
                if (value == _isChecked) return;
                _isChecked = value;
                this.SetIsChecked(value, true, true);
                OnPropertyChanged("IsChecked");
            }
        }

        public void Add(ITreeItemViewModel item)
        {
            _items.Add(item);
            item.Parent = this;
        }

        public bool Remove(ITreeItemViewModel item) { return _items.Remove(item); }

        protected virtual void OnItemsChanged() { }

        #region IEquatable
        public bool Equals(TreeItemViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as TreeItemViewModel;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return _id.GetHashCode();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        // with .NET 4.5 you may use [CallerMemberName]
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeItemViewModel<TItem> : TreeItemViewModel, ITreeItemViewModel<TItem> 
        where TItem : ITreeItemViewModel
    {
        public new TItem Parent
        {
            get { return (TItem) base.Parent; }
            set { base.Parent = value; }
        }

        public new IEnumerable<TItem> Items
        {
            get { return base.Items.Cast<TItem>(); }
            set
            {
                var items = value?.Cast<ITreeItemViewModel>();
                base.Items = items;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeViewModel : TreeItemViewModel, ITreeViewModel
    {
        private ITreeItemViewModel _selectedItem;

        protected override void OnItemsChanged()
        {
            SelectedItem = null;
        }

        public ITreeItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(_selectedItem, value)) return;
                if (_selectedItem != null) _selectedItem.IsSelected = false;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");

                if (_selectedItem != null) 
                    _selectedItem.IsSelected = true;
            }
        }

        // Binding SelectedItem on WPF TreeView (with Caliburn.Micro), 
        // cf: https://caliburnmicro.codeplex.com/discussions/243108
        public virtual void SelectedItemChanged(TreeView treeView)
        {
            if (treeView == null) throw new ArgumentNullException(nameof(treeView));
            SelectedItem = treeView.SelectedItem as ITreeItemViewModel;
        }
    }

    [ExcludeFromCodeCoverage]
    public class TreeViewModel<TItem> : TreeItemViewModel<TItem>, ITreeViewModel<TItem> 
        where TItem : class, ITreeItemViewModel
    {
        private TItem _selectedItem;

        protected override void OnItemsChanged()
        {
            SelectedItem = null;
        }

        public TItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(_selectedItem, value)) return;
                if (_selectedItem != null) _selectedItem.IsSelected = false;
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");

                if (_selectedItem != null)
                    _selectedItem.IsSelected = true;
            }
        }

        // Binding SelectedItem on WPF TreeView (with Caliburn.Micro), 
        // cf: https://caliburnmicro.codeplex.com/discussions/243108
        public virtual void SelectedItemChanged(TreeView treeView)
        {
            if (treeView == null) throw new ArgumentNullException(nameof(treeView));
            SelectedItem = treeView.SelectedItem as TItem;
        }
    }

    [ExcludeFromCodeCoverage]
    public static class TreeItemViewExtensions
    {
        #region Checked state handling copied from http://www.codeproject.com/Articles/28306/Working-with-Checkboxes-in-the-WPF-TreeView

        public static void SetIsChecked(this ITreeItemViewModel viewModel, bool? value, bool updateChildren, bool updateParent)
        {
            var isChecked = value;

            if (updateChildren && viewModel.IsChecked.HasValue)
            {
                viewModel.Items.ToList().ForEach(c => c.SetIsChecked(isChecked, true, false));
            }

            if (updateParent && viewModel.Parent != null)
                viewModel.Parent.VerifyCheckState();

            viewModel.IsChecked = isChecked;
        }

        public static void VerifyCheckState(this ITreeItemViewModel viewModel)
        {
            bool? state = null;
            var items = viewModel.Items.ToList();
            for (var i = 0; i < items.Count; ++i)
            {
                var current = items[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            viewModel.SetIsChecked(state, false, true);
        }

        #endregion

        public static TItem FindFirst<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> predicate) 
            where TItem : class, ITreeItemViewModel
        {
            if (items == null) return null;

            foreach (var item in items)
            {
                if (predicate(item)) 
                    return item;
                // depth-first traversal
                var found = FindFirst(item.Items.OfType<TItem>(), predicate);
                if (found != null) 
                    return found;
            }
            return null;
        }

        public static IEnumerable<TItem> FindAll<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> predicate)
            where TItem : class, ITreeItemViewModel
        {
            if (items == null) return Enumerable.Empty<TItem>();

            var result = new List<TItem>();

            foreach (var item in items)
            {
                if (predicate(item))
                    result.Add(item);
                // depth-first traversal
                result.AddRange(FindAll(item.Items.OfType<TItem>(), predicate));
            }
            return result;
        }

        public static void Apply<TItem>(this IEnumerable<TItem> items, Action<TItem> action)
            where TItem : class, ITreeItemViewModel
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                action(item);
                Apply(item.Items.OfType<TItem>(), action);
            }
        }
    }
}