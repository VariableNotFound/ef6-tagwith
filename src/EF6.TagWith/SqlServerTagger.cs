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

            var startsWithAnd = CmpStr(sql, predicateStartIndex - 5, "AND (");
            var endsWithAnd = CmpStr(sql, endOfTagIndex + 2, ") AND");

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
                finalSql = sql.Substring(0, indexOfTagConstant - 8)
                           + sql.Substring(predicateEndIndex + 1);
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

        private static bool CmpStr(string str, int startIndex, string compare)
        {
            var index = 0;
            if (index + startIndex >= str.Length)
                return false;

            while ((index+startIndex) < str.Length && index < compare.Length)
            {
                if (str[index + startIndex] != compare[index])
                    return false;
                index++;
            }
            return true;
        }
    }
}