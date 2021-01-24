using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6.TagWith
{
    public class QueryTaggerInterceptor : DbCommandInterceptor
    {
        private readonly ISqlTagger _sqlTagger;
        private readonly Action<string> _sqlWriter = null;
        private readonly TaggingOptions _options = new TaggingOptions();

        public QueryTaggerInterceptor(ISqlTagger sqlTagger)
        {
            _sqlTagger = sqlTagger;
            TagWith.IsInitialized = true;
        }

        public QueryTaggerInterceptor(ISqlTagger sqlTagger, TaggingOptions options): this(sqlTagger)
        {
            _options = options;
        }

        internal QueryTaggerInterceptor(ISqlTagger sqlTagger, TaggingOptions options, Action<string> sqlWriter): this(sqlTagger, options)
        {
            _sqlWriter = sqlWriter;
        }
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> context)
        {
            while(true)
            {
                var newSqlQuery = _sqlTagger.GetTaggedSqlQuery(command.CommandText, _options);
                if (newSqlQuery.Equals(command.CommandText))    // The query didn't change,
                    break;                                      // so it is not tagged
                command.CommandText = newSqlQuery;
            }
            _sqlWriter?.Invoke(command.CommandText);
        }
    }
}
