using System;

namespace HelloAggregations
{
    public class Athlete
    {
        public string Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public Location Location { get; set; }
        public int[] Rating { get; set; }
        public string Sport { get; set; }
        
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}