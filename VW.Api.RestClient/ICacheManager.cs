using System;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The cache manager Interface 
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Gets or sets the default cache time in minutes.
        /// </summary>
        int DefaultCacheTimeInMinutes { get; }

        /// <summary>
        /// Sets the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheTimeInMinutes">The cache time in minutes.</param>
        void Set<T>(string name, T value, int? cacheTimeInMinutes = null);

        /// <summary>
        /// Sets the string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheTimeInMinutes">The cache time in minutes.</param>
        void SetString(string name, string value, int? cacheTimeInMinutes = null);

        /// <summary>
        /// Gets the specified name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        T Get<T>(string name, T defaultValue = default(T));

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        string GetString(string name);

        /// <summary>
        /// Gets the or set.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="cacheTimeInMinutes">The cache time in minutes.</param>
        /// <returns></returns>
        T GetOrSet<T>(string name, Func<T> callback, int? cacheTimeInMinutes = null);

        /// <summary>
        /// Removes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        void Remove(string name);

    }
}