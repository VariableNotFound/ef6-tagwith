using System;
using System.Data.Entity;
using System.Linq;
using EF6.TagWith.Tests.Data;
using Xunit;

namespace EF6.TagWith.Tests
{
    [Collection("Tags")]
    public class SqlServerQueryPrefixTests : IDisposable
    {
        private readonly DataContext _ctx;
        private string _lastSql;

        public SqlServerQueryPrefixTests()
        {
            _ctx = new DataContext();
            _ctx.Database.CreateIfNotExists();
            DbInterceptionUtils.AddInterceptorAndRemovePrevious(
                new QueryTaggerInterceptor(new SqlServerTagger(), new TaggingOptions(), s => _lastSql = s));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryOnlyTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = "This is the tag";
            var items = _ctx.Friends.TagWith(tag).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryOnlyMultipleTagWiths(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
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

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryStartingWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(QueryStartingWithTagWith);
            var items = _ctx.Friends.TagWith(tag).Where(f => f.Id < 10).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryEndingWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(QueryEndingWithTagWith);
            var items = _ctx.Friends.Where(f => f.Id < 10).TagWith(tag).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryMiddleWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(QueryMiddleWithTagWith);
            var items = _ctx.Friends.Where(f => f.Id < 10).TagWith(tag).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryWithIncludesWithMiddleTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(QueryWithIncludesWithMiddleTagWith);
            var items = _ctx.Friends.Include("Tags").Include("Country").Where(f => f.Id < 10).TagWith(tag).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryWithIncludesStartingWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(QueryWithIncludesStartingWithTagWith);
            var items = _ctx.Friends.TagWith(tag).Include("Tags").Include("Country").Where(f => f.Id < 10).Where(f => f.Id < 3).ToList();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void FirstOrDefaultOrderedStartingWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(FirstOrDefaultOrderedStartingWithTagWith);
            var item = _ctx.Friends.TagWith(tag).OrderBy(f => f.Name).FirstOrDefault(f => f.Id < 3);
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void FirstOrDefaultOrderedEndingWithTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = nameof(FirstOrDefaultOrderedEndingWithTagWith);
            var item = _ctx.Friends.Where(f => f.Id < 3).TagWith(tag).FirstOrDefault();
            Assert.True(IsSqlTagged(_lastSql, tag));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryWithMultipleConsecutiveTagWiths(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var query = _ctx.Friends.Where(f => f.Id < 3).TagWith(tag1);
            var items = query.TagWith(tag2).ToList();

            Assert.True(IsSqlTagged(_lastSql, tag1, tag2));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag1));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag2));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void QueryWithMultipleSeparatedTagWith(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var items = _ctx.Friends.TagWith(tag1).Where(f => f.Id < 3).TagWith(tag2).ToList();

            Assert.True(IsSqlTagged(_lastSql, tag1, tag2));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag1));
            Assert.False(SqlContainsMarkerPredicate(_lastSql, tag2));
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void TagsAreInsertedInTheSameOrder(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag1 = "First tag";
            var tag2 = "Second tag";
            var tag3 = "Third tag";
            var query = _ctx.Friends.TagWith(tag1).Where(f => f.Id < 3).TagWith(tag2);
            query = query.Where(f => f.Country.Id == 34).TagWith(tag3);
            var items = query.ToList();
            Assert.StartsWith($"-- {tag1}\n-- {tag2}\n-- {tag3}", _lastSql);
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void TagWithSourceIncludesCallerInfo(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var items = _ctx.Friends.TagWithSource().ToList();
            Assert.Contains(nameof(TagWithSourceIncludesCallerInfo), _lastSql);
        }

        [Theory]
        [InlineData(PredicateExpression.Equals)]
        [InlineData(PredicateExpression.NotEquals)]
        public void TagWithSourceIncludesCallerInfoAndTag(PredicateExpression predicateExpression)
        {
            InitializeInterceptor(predicateExpression);
            var tag = "xyz";
            var items = _ctx.Friends.TagWithSource(tag).ToList();
            Assert.Contains(tag, _lastSql);
            Assert.Contains(nameof(TagWithSourceIncludesCallerInfoAndTag), _lastSql);
        }

        public void Dispose()
        {
            _ctx?.Dispose();
        }

        // Checks if the SQL sentence contains a tag comment
        private bool IsSqlTagged(string sql, params string[] tags) =>
            (tags.All(tag => sql.IndexOf($"-- {tag}\n", StringComparison.Ordinal) > -1));

        // Checks if the SQL sentence contains the marker predicate
        private bool SqlContainsMarkerPredicate(string sql, string tag) =>
            sql.IndexOf($"'{TagWithExtensions.TagMarker}' = '{tag}'", StringComparison.Ordinal) > -1;

        private void InitializeInterceptor(PredicateExpression predicateExpression)
        {
            DbInterceptionUtils.AddInterceptorAndRemovePrevious(
                new QueryTaggerInterceptor(new SqlServerTagger(), new TaggingOptions() { TagMode = TagMode.Prefix, PredicateExpression = predicateExpression }, s => _lastSql = s));
        }
    }
}
