using System;
using Caliburn.Micro;

namespace HelloFacets
{
    public class DocumentViewModel : Screen, IDocument
    {
        private readonly Document _document;
        private string _highlight = String.Empty;

        public DocumentViewModel(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");
            _document = document;

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisplayName = _document.Title;
        }

        public Guid Id { get { return _document.Id; } }
        public string Type { get { return _document.Type; } }

        public string Title
        {
            get { return _document.Title; }
            set
            {
                if (_document.Title == value) return;
                _document.Title = value;
                NotifyOfPropertyChange();
                DisplayName = value;
            }
        }

        public string Highlight
        {
            get { return _highlight; }
            set
            {
                if (value == _highlight) return;
                _highlight = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime? Created
        {
            get { return _document.Created; }
            set
            {
                if (_document.Created == value) return;
                _document.Created = value;
                NotifyOfPropertyChange();
            }
        }

        public DateTime? Changed
        {
            get { return _document.Changed; }
            set
            {
                if (_document.Changed == value) return;
                _document.Changed = value;
                NotifyOfPropertyChange();
            }
        }
        
        public string Content
        {
            get { return _document.Content; }
            set
            {
                if (_document.Content == value) return;
                _document.Content = value;
                NotifyOfPropertyChange();
            }
        }

        public Location Location
        {
            get { return _document.Location; }
            set
            {
                if (_document.Location == value) return;
                _document.Location = value;
                NotifyOfPropertyChange();
            }
        }

        public TimeSpan? TimeToLive
        {
            get { return _document.TimeToLive; }
            set
            {
                if (_document.TimeToLive == value) return;
                _document.TimeToLive = value;
                NotifyOfPropertyChange();
            }
        }
    }
}