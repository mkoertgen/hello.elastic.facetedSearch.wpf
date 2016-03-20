using System;
using Nest;

namespace HelloNest.Tests
{
    [ElasticsearchType(Name = "document", IdProperty = "Id")]
    public class Document
    {
        public Guid Id { get; private set; }

        [Nested(IncludeInParent = true)]
        public Attachment File { get; set; }

        public Document()
        {
            Id = Guid.NewGuid();
        }
    }
}