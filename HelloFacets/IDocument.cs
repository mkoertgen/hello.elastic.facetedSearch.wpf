using System;

namespace HelloFacets
{
    public interface IDocument
    {
        Guid Id { get; }
        string Title { get; set; }
        string Type { get; }
        DateTime? Created { get; set; }
        DateTime? Changed { get; set; }
        string Content { get; set; }
        Location Location { get; set; }
        TimeSpan? TimeToLive { get; set; }
    }
}