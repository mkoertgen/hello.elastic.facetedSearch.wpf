using System;

namespace HelloFacets
{
    public class Document : IDocument
    {
        public Guid Id { get; private set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Changed { get; set; }
        public string Content { get; set; }
        public Location Location { get; set; }
        public TimeSpan? TimeToLive { get; set; }

        public Document()
        {
            Id = Guid.NewGuid();
            Title = String.Empty;
            Type = String.Empty;
            Created = DateTime.UtcNow;
            Changed = DateTime.UtcNow;
            Content = String.Empty;
            //TimeToLive = TimeSpan.FromDays(3); // default is specified in mapping
        }
    }
}