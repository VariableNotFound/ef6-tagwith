using System;
using System.Linq;
using System.Linq.Expressions;
using EF6.TagWith.Tests.Data;
using Xunit;

namespace EF6.TagWith.Tests
{
    public class IQueryableExtensionTests
    {
        [Fact]
        public void UsingTagWithRequiresInitialization()
        {
            TagWith.IsInitialized = false;
            Assert.Throws<InvalidOperationException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith(null).ToList();
            });
        }

        [Fact]
        public void TagCanNotBeNull()
        {
            TagWith.IsInitialized = true;
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith(null).ToList();
            });
        }

        [Fact]
        public void TagCanNotBeEmptyString()
        {
            TagWith.IsInitialized = true;
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith(string.Empty).ToList();
            });
        }

        [Fact]
        public void TagCanNotBeSpaces()
        {
            TagWith.IsInitialized = true;
            Assert.Throws<ArgumentException>(() =>
            {
                var items = Enumerable.Empty<Friend>().AsQueryable().TagWith("  ").ToList();
            });
        }
    }
}