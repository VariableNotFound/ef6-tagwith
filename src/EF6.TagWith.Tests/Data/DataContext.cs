using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EF6.TagWith.Tests.Data;

namespace EF6.TagWith.Tests.Data
{
    public class DataContext : DbContext
    {
        public string Tag { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}