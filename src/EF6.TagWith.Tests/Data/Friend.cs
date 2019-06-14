using System.Collections.Generic;

namespace EF6.TagWith.Tests.Data
{
    public class Friend
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Country Country { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
}