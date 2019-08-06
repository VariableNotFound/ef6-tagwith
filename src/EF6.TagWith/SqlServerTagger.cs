using System;

namespace EF6.TagWith
{
    public class SqlServerTagger: ISqlTagger
    {
        public string GetTaggedSqlQuery(string sql)
        {
            var indexOfTagConstant = sql.IndexOf(
                TagWithExtensions.TagMarker, StringComparison.Ordinal
            );
            if (indexOfTagConstant == -1)
                return sql;

            var predicateStartIndex = indexOfTagConstant - 2;
            var startOfTagIndex = predicateStartIndex + TagWithExtensions.TagMarker.Length + 8;

            var predicateEndIndex = sql.IndexOf("'", startOfTagIndex, StringComparison.Ordinal);
            var endOfTagIndex = predicateEndIndex - 1; // Remove the final single quote

            var tag = sql.Substring(startOfTagIndex, endOfTagIndex - startOfTagIndex + 1);

            const string startingAnd = "AND (";
            var startsWithAnd = StringIsAtIndex(sql, predicateStartIndex - 5, startingAnd);
            const string endingAnd = ") AND";
            var endsWithAnd = StringIsAtIndex(sql, endOfTagIndex + 2, endingAnd);

            string finalSql;
            if (startsWithAnd)
            {
                // Predicate pattern: ... AND (N'__tag' = N'mytag')
                finalSql = sql.Substring(0, indexOfTagConstant - 8) 
                           + sql.Substring(endOfTagIndex + 3);
            }
            else if (endsWithAnd)
            {
                // Predicate pattern: (N'__tag' = N'mytag') AND ...
                finalSql = sql.Substring(0, indexOfTagConstant - 3) 
                           + sql.Substring(endOfTagIndex + 8);
            }
            else
            {
                // It is the only predicate, so remove the whole "Where" section

                // In some cases the predicate can be wrapped with parentheses even if it's the only predicate left in
                // the query
                var hasParens = StringIsAtIndex(sql, predicateStartIndex - 1, "(");
                var startIndex = indexOfTagConstant - (hasParens ? 9 : 8);
                var endIndex = predicateEndIndex + (hasParens ? 2 : 1);
                finalSql = sql.Substring(0, startIndex) + sql.Substring(endIndex);
            }

            finalSql = AddTagToQuery(finalSql, tag);
            return finalSql;
        }

        private string AddTagToQuery(string sql, string tag)
        {
            if (sql.StartsWith("-- ")) 
            {
                // There are another comments in the query
                var selectIndex = sql.IndexOf("SELECT ", StringComparison.Ordinal);
                if (selectIndex > -1)
                {
                    sql = sql.Substring(0, selectIndex)
                               + $"-- {tag}\n" + sql.Substring(selectIndex);
                }
            }
            else
            {
                // This is the first tag
                sql = $"-- {tag}\n{sql}"; 
            }

            return sql;
        }

        private static bool StringIsAtIndex(string str, int startIndex, string compare)
        {
            if (startIndex + compare.Length > str.Length)
                return false;
            return str.IndexOf(compare, startIndex, compare.Length) == startIndex;
        }
    }
}