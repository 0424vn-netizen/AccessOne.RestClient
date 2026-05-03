using System;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The json serializer provider
    /// </summary>
    [Serializable]
    public enum JsonSerializerProvider
    {
        /// <summary>
        /// The default restsharp serializer
        /// </summary>
        Default = 0,

        /// <summary>
        /// The newtonsoft json serializer
        /// </summary>
        Newtonsoft = 1
    }
}