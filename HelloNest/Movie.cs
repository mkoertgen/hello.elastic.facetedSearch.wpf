using System;

namespace HelloNest
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Director { get; set; }
        public int Year { get; set; }
        public string[] Genres { get; set; }

        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }

        public string Content { get; set; }

        public Person[] Actors { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}