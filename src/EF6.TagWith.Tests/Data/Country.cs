﻿using System.Collections.Generic;

namespace EF6.TagWith.Tests.Data
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Friend> People { get; set; }
    }
}