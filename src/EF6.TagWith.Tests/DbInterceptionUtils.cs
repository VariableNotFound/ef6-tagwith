using System.Data.Entity.Infrastructure.Interception;

namespace EF6.TagWith.Tests
{
    public class DbInterceptionUtils
    {
        private static QueryTaggerInterceptor _currentInterceptor;

        public static void AddInterceptorAndRemovePrevious(QueryTaggerInterceptor interceptor)
        {
            // DbInterception is static so we must run tests sequentially and ensure
            // we remove the previous interceptor before adding a new one
            if (_currentInterceptor != null)
            {
                DbInterception.Remove(_currentInterceptor);
            }
            DbInterception.Add(interceptor);
            _currentInterceptor = interceptor;
        }

    }
}