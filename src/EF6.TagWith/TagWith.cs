using System.Data.Entity.Infrastructure.Interception;
using System.Security.Cryptography.X509Certificates;

namespace EF6.TagWith
{
    public static class TagWith
    {
        internal static bool IsInitialized { get; set; }
        internal static TaggingOptions Options { get; set; }

        public static void Initialize<TTagger>(TaggingOptions options = null) where TTagger : ISqlTagger, new()
        {
            Options = options ?? new TaggingOptions();
            DbInterception.Add(new QueryTaggerInterceptor(new TTagger(), Options));
        }
    }
}