namespace AybJetProviderApi.Services.Cache
{
    public interface ICache
    {
        T? Get<T>(string key);
        void Set<T>(string key, T value);
        void Remove(string key);
    }
} 