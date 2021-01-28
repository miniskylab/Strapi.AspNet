using System.Collections.Generic;

namespace Strapi.AspNet.Engine
{
    internal static class InMemoryCache
    {
        static readonly Dictionary<string, object> CacheStore;

        static InMemoryCache() { CacheStore = new Dictionary<string, object>(); }

        public static void Store(string key, object @object) { CacheStore.Add(key, @object); }

        public static bool TryGet(string key, out object value) { return CacheStore.TryGetValue(key, out value); }
    }
}