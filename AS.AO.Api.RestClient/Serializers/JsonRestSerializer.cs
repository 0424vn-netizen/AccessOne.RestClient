using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.IO;
using System.Threading;

namespace AS.AO.Api.RestClient.Serializers
{
    /// <summary>
    /// The newtonsoft json serializer
    /// </summary>
    /// <seealso cref="ISerializer" />
    /// <seealso cref="IDeserializer" />
    public class JsonRestSerializer : ISerializer, IDeserializer
    {
        #region Fields & Properties

        /// <summary>
        /// The content type application json
        /// </summary>
        private const string ContentTypeApplicationJson = "application/json";

        /// <summary>
        /// The default instance holder.
        /// </summary>
        private static readonly Lazy<JsonRestSerializer> _default = new Lazy<JsonRestSerializer>(LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// The serializer implementation.
        /// </summary>
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        /// <summary>
        /// Gets the default NewtonsoftJsonSerializer instance.
        /// </summary>
        public static JsonRestSerializer Default => _default.Value;

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string DateFormat { get; set; }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string RootElement { get; set; }

        /// <summary>
        /// Unused for JSON Serialization
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Content type for serialized content	
        /// </summary>
        public string ContentType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
        /// </summary>
        public JsonRestSerializer()
        {
            this.ContentType = ContentTypeApplicationJson;
            this._serializer = JsonSerializer.DefaultJsonSerializer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        public JsonRestSerializer(Newtonsoft.Json.JsonSerializer serializer)
        {
            this.ContentType = ContentTypeApplicationJson;
            _serializer = serializer;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        /// <param name="obj">Object to serialize</param>
        /// <returns>JSON as String</returns>
        public string Serialize(object obj)
        {
            using (var stringWriter = new StringWriter())
            {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter))
                {
                    _serializer.Serialize(jsonTextWriter, obj);

                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Deserializes the specified response.
        /// </summary>
        /// <typeparam name="T">The response type.</typeparam>
        /// <param name="response">The response.</param>
        /// <returns>
        /// The strongly-typed deserialized response.
        /// </returns>
        public T Deserialize<T>(IRestResponse response)
        {
            if (response == null)
            {
                return default(T);
            }

            using (var stringReader = new StringReader(response.Content))
            {
                using (var jsonTextReader = new JsonTextReader(stringReader))
                {
                    return _serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        #endregion
    }
}
