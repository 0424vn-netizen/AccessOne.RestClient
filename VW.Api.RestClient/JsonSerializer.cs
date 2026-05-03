using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.IO;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The newtonsoft json serializer settings
    /// </summary>
    public static class JsonSerializer
    {
        #region Properties

        /// <summary>
        /// Gets the default json serializer.
        /// </summary>
        public static Newtonsoft.Json.JsonSerializer DefaultJsonSerializer { get; private set; }

        /// <summary>
        /// Gets the default json serializer settings.
        /// </summary>
        public static JsonSerializerSettings DefaultJsonSerializerSettings { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="JsonSerializer"/> class.
        /// </summary>
        static JsonSerializer()
        {
            DefaultJsonSerializer = new Newtonsoft.Json.JsonSerializer
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            DefaultJsonSerializerSettings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            DefaultJsonSerializer.Converters.Add(new StringEnumConverter());
            DefaultJsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Serializes an object as json string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string Serialize<T>(T value, Formatting formatting = Formatting.None)
        {
            return Serialize(value, DefaultJsonSerializerSettings, formatting);
        }

        /// <summary>
        /// Serializes an object as json string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string Serialize<T>(T value, JsonSerializerSettings serializerSettings, Formatting formatting = Formatting.None)
        {
            return value == null ? null : JsonConvert.SerializeObject(value, formatting, serializerSettings);
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string SerializeObject(object value, Formatting formatting = Formatting.None)
        {
            return SerializeObject(value, DefaultJsonSerializerSettings, formatting);
        }

        /// <summary>
        /// Serializes an object as json string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string SerializeObject(object value, JsonSerializerSettings serializerSettings, Formatting formatting = Formatting.None)
        {
            return value == null ? null : JsonConvert.SerializeObject(value, formatting, serializerSettings);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string value)
        {
            return Deserialize<T>(value, DefaultJsonSerializerSettings);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string value, JsonSerializerSettings serializerSettings)
        {
            return string.IsNullOrWhiteSpace(value) ? default(T) : JsonConvert.DeserializeObject<T>(value, serializerSettings);
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            return Deserialize<T>(File.ReadAllText(filePath), DefaultJsonSerializerSettings);
        }

        /// <summary>
        /// Deserializes from file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="serializerSettings">The serializer settings.</param>
        /// <returns></returns>
        public static T DeserializeFromFile<T>(string filePath, JsonSerializerSettings serializerSettings)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            return Deserialize<T>(File.ReadAllText(filePath), serializerSettings);
        }

        #endregion

    }
}