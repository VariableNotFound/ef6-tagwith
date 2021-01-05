using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EF6.TagWith
{
    public static class TagWithExtensions
    {
        public static string TagMarker = "!__tag!";

        public static IQueryable<T> TagWith<T>(this IQueryable<T> query, string tag)
        {
            EnsureTagWithIsConfigured();
            EnsureTagIsValid(tag);
            var tagConstant = Expression.Constant(TagMarker);
            var tagValue = Expression.Constant(tag);
            var equalsExpression = Expression.Equal(tagConstant, tagValue);
            var predicate = Expression.Lambda<Func<T, bool>>(
                equalsExpression, Expression.Parameter(typeof(T))
            );
            return query.Where(predicate);
        }

        private static void EnsureTagWithIsConfigured()
        {
            if(!EF6.TagWith.TagWith.IsInitialized)
                throw new InvalidOperationException("TagWith component must be initialized before using it in a query. Consider calling TagWith.Initialize(...) on your application startup.");
        }

        public static IQueryable<T> TagWithSource<T>(this IQueryable<T> queryable,
            string tag = "",
            [CallerMemberName] string methodName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int line = 0)
        {
            return queryable.TagWith(string.IsNullOrEmpty(tag)
                ? $"{methodName} - {sourceFilePath}:{line}"
                : $"{tag} - {methodName} - {sourceFilePath}:{line}");
        }

        private static void EnsureTagIsValid(string tag)
        {
            if(string.IsNullOrWhiteSpace(tag))
                throw new ArgumentException("A tag must be specified");

            if (tag.IndexOf("'", StringComparison.Ordinal) > -1)
                throw new ArgumentException("Tags can not contain single quotes (')");
        }
    }

}
