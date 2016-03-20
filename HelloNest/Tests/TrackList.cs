namespace HelloNest.Tests
{
    public class TrackList : IHaveType
    {
        public string Type { get; }

        public TrackList()
        {
            Type = GetType().AssemblyQualifiedName;
        }
    }
}