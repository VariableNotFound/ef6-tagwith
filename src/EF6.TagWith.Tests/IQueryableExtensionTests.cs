using System;
using System.Linq;
using EF6.TagWith.Tests.Data;
using Xunit;

namespace EF6.TagWith.Tests
{
    public class IQueryableExtensionTests
    {
        [Fact]
        public void TagCanNotBeNull()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith(null).ToList();
            });
        }

        [Fact]
        public void TagCanNotBeEmptyString()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith(string.Empty).ToList();
            });
        }

        [Fact]
        public void TagCanNotBeSpaces()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith("  ").ToList();
            });
        }

    }
}