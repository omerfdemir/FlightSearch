using System.Collections.Concurrent;

namespace AybJetProviderApi.Services.Cache
{
    public class MemoryCache : ICache
    {
        private readonly ConcurrentDictionary<string, object> _cache = new();

        public T? Get<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        public void Set<T>(string key, T value)
        {
            _cache.AddOrUpdate(key, value!, (_, _) => value!);
        }

        public void Remove(string key)
        {
            _cache.TryRemove(key, out _);
        }
    }
} 