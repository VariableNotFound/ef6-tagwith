using System.Data.Entity.Infrastructure.Interception;
using System.Security.Cryptography.X509Certificates;

namespace EF6.TagWith
{
    public static class TagWith
    {
        public static bool IsInitialized { get; internal set; }

        public static void Initialize<TTagger>(TaggingOptions options = null) where TTagger : ISqlTagger, new()
        {
            DbInterception.Add(new QueryTaggerInterceptor(new TTagger(), options ?? new TaggingOptions()));
            IsInitialized = true;
        }
    }
}