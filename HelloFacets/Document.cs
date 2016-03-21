using System;

namespace HelloFacets
{
    public class Document : IDocument
    {
        public Guid Id { get; }
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
            Title = string.Empty;
            Type = string.Empty;
            Created = DateTime.UtcNow;
            Changed = DateTime.UtcNow;
            Content = string.Empty;
            //TimeToLive = TimeSpan.FromDays(3); // default is specified in mapping
        }
    }
}