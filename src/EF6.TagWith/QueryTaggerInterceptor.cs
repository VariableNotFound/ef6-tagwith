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

        public QueryTaggerInterceptor(ISqlTagger sqlTagger)
        {
            _sqlTagger = sqlTagger;
        }

        internal QueryTaggerInterceptor(ISqlTagger sqlTagger, Action<string> sqlWriter)
        {
            _sqlTagger = sqlTagger;
            _sqlWriter = sqlWriter;
        }
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> context)
        {
            do
            {
                command.CommandText = _sqlTagger.GetTaggedSqlQuery(command.CommandText);
            } while (command.CommandText.IndexOf(TagWithExtensions.TagMarker, StringComparison.Ordinal) > -1);
            _sqlWriter?.Invoke(command.CommandText);
        }
    }
}
