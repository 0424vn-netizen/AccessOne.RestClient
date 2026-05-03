using System;
using System.Runtime.Caching;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The file caching manager
    /// </summary>
    public static class FileCachingManager
    {
        /// <summary>
        /// The instance name
        /// </summary>
        private static readonly string _instanceName = Guid.NewGuid().ToString();

        /// <summary>
        /// Sets the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="filePath">The file path.</param>
        public static void Set<T>(string name, T value, string filePath)
        {
            if (value == null)
            {
                return;
            }

            var policy = new CacheItemPolicy();
            policy.ChangeMonitors.Add(new HostFileChangeMonitor(new[] { filePath }));

            var cacheKey = $"{_instanceName}_{name}";
            MemoryCache.Default.Set(cacheKey, value, policy);
        }

        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T Get<T>(string name, T defaultValue = default(T))
        {
            var cacheKey = $"{_instanceName}_{name}";
            var cachedValue = MemoryCache.Default[cacheKey];
            if (cachedValue != null && cachedValue is T value)
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the or set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static T GetOrSet<T>(string name, Func<string, T> callback, string filePath)
        {
            var cacheKey = $"{_instanceName}_{name}";
            var cachedValue = MemoryCache.Default[cacheKey];
            if (cachedValue != null && cachedValue is T value)
            {
                return value;
            }

            value = callback(filePath);
            Set(name, value, filePath);

            return value;
        }
    }
}