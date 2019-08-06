using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using EF6.TagWith.Tests.Data;
using Xunit;

namespace EF6.TagWith.Tests
{
    public class SqlServerQueryTests : IDisposable
    {
        private readonly DataContext _ctx;
        private string _lastSql;

        public SqlServerQueryTests()
        {
            _ctx = new DataContext();
            _ctx.Database.CreateIfNotExists();
            DbInterception.Add(new QueryTaggerInterceptor(new SqlServerTagger(), s => _lastSql = s));
        }

        [Fact]
        public void QueryOnlyTagWith()
        {
            var tag = nameof(QueryOnlyTagWith);
            var items = _ctx.Friends.TagWith(tag).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void QueryOnlyMultipleTagWiths()
        {
            var tag1 = nameof(QueryOnlyMultipleTagWiths);
            var tag2 = "Second tag";

            var query = _ctx.Friends
                .TagWith(tag1)
                .TagWith(tag2)
                .ToList();

            Assert.True(IsSqlTagged(_lastSql, tag1));
            Assert.True(IsSqlTagged(_lastSql, tag2));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag1));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag2));
            Assert.StartsWith($"-- {tag1}\n-- {tag2}\n", _lastSql);
        }

        [Fact]
        public void QueryStartingWithTagWith()
        {
            var tag = nameof(QueryStartingWithTagWith);
            var items = _ctx.Friends.TagWith(tag).Where(f => f.Id < 10).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void QueryEndingWithTagWith()
        {
            var tag = nameof(QueryEndingWithTagWith);
            var items = _ctx.Friends.Where(f => f.Id < 10).TagWith(tag).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void QueryMiddleWithTagWith()
        {
            var tag = nameof(QueryMiddleWithTagWith);
            var items = _ctx.Friends.Where(f => f.Id < 10).TagWith(tag).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void QueryWithIncludesWithMiddleTagWith()
        {
            var tag = nameof(QueryWithIncludesWithMiddleTagWith);
            var items = _ctx.Friends.Include("Tags").Include("Country").Where(f => f.Id < 10).TagWith(tag).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void QueryWithIncludesStartingWithTagWith()
        {
            var tag = nameof(QueryWithIncludesStartingWithTagWith);
            var items = _ctx.Friends.TagWith(tag).Include("Tags").Include("Country").Where(f => f.Id < 10).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void FirstOrDefaultOrderedStartingWithTagWith()
        {
            var tag = nameof(FirstOrDefaultOrderedStartingWithTagWith);
            var item = _ctx.Friends.TagWith(tag).OrderBy(f => f.Name).FirstOrDefault(f => f.Id < 3);
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Fact]
        public void FirstOrDefaultOrderedEndingWithTagWith()
        {
            var tag = nameof(FirstOrDefaultOrderedEndingWithTagWith);
            var item = _ctx.Friends.Where(f => f.Id < 3).TagWith(tag).FirstOrDefault();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }


        [Fact]
        public void QueryWithMultipleConsecutiveTagWiths()
        {
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var query = _ctx.Friends.Where(f => f.Id < 3).TagWith(tag1);
            var items = query.TagWith(tag2).ToList();

            Assert.True(IsSqlTagged(_lastSql, tag1, tag2));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag1));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag2));
        }

        [Fact]
        public void QueryWithMultipleSeparatedTagWith()
        {
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var items = _ctx.Friends.TagWith(tag1).Where(f => f.Id < 3).TagWith(tag2).ToList();

            Assert.True(IsSqlTagged(_lastSql, tag1, tag2));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag1));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag2));
        }

        [Fact]
        public void TagsAreInsertedInTheSameOrder()
        {
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var tag3 = "Third tag";
            var query = _ctx.Friends.TagWith(tag1).Where(f => f.Id < 3).TagWith(tag2);
            query = query.Where(f => f.Country.Id == 34).TagWith(tag3);
            var items = query.ToList();
            Assert.StartsWith($"-- {tag1}\n-- {tag2}\n-- {tag3}", _lastSql);
        }

        // Checks if the SQL sentence contains a tag comment
        private bool IsSqlTagged(string sql, params string[] tags) =>
            (tags.All(tag=>sql.IndexOf($"-- {tag}\n", StringComparison.Ordinal) > -1));

        // Checks if the SQL sentence contains the marker predicate
        private bool SqlContainsMarkerPredicate(string sql, string tag) =>
            sql.IndexOf($"'{TagWithExtensions.TagMarker}' = '{tag}'", StringComparison.Ordinal) > -1;

        public void Dispose()
        {
            _ctx?.Dispose();
        }
    }
}
