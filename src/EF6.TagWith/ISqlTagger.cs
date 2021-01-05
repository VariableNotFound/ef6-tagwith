namespace EF6.TagWith
{
    public interface ISqlTagger
    {
        string GetTaggedSqlQuery(string sql, TaggingOptions options);
    }
}