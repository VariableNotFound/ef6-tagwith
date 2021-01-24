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
        private readonly Action<string> _sqlWriter;
        private readonly TaggingOptions _options;

        public QueryTaggerInterceptor(ISqlTagger sqlTagger, TaggingOptions options = null)
        {
            _sqlTagger = sqlTagger ?? throw new ArgumentNullException(nameof(sqlTagger));
            _options = options ?? new TaggingOptions();
            TagWith.IsInitialized = true;
        }

        internal QueryTaggerInterceptor(ISqlTagger sqlTagger, TaggingOptions options, Action<string> sqlWriter): this(sqlTagger, options)
        {
            _sqlWriter = sqlWriter ?? throw new ArgumentNullException(nameof(sqlWriter));
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
