namespace HelloNest.Tests
{
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public TrackList Tracks { get; set; }
    }

    public class LpTrackList : TrackList
    {
        public string TracksSideA { get; set; }
        public string TracksSideB { get; set; }
    }

    public class CdTrackList : TrackList
    {
        public string Tracks { get; set; }
    }
}