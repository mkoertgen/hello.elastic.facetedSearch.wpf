using System;
using System.IO;
using Nest;

namespace HelloNest.Tests
{
    [ElasticsearchType(Name = "attachment", IdProperty = "Id")]
    public class Attachment
    {
        public Guid Id { get; private set; }

        public Attachment()
        {
            Id = Guid.NewGuid();
        }

        [Binary(Name = "_content")]
        public byte[] Content { get; set; }

        [String(Name = "_content_type")]
        public string ContentType { get; set; }

        [String(Name = "_name")]
        public string Name { get; set; }

        public static Attachment CreateFromFile(string path)
        {
            return new Attachment
            {
                Content = File.ReadAllBytes(path),
                ContentType = "image/png",
                Name = Path.GetFileName(path)
            };
        }

        public void SaveToFile(string path)
        {
            File.WriteAllBytes(path, Content);
        }
    }
}