using System;

namespace ExpressoReporting.MobileAppService.Services
{
    public interface ICacheService
    {
        int GetCacheTimeout(string cachePrefix, string cacheSuffixPattern = null);
        void Set<TItem>(string key, TItem value, DateTimeOffset absoluteExpiration);
        void Remove(string key);
        object Get(string key);
    }
}
