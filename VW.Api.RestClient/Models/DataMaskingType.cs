using System;

namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The Data Masking Type
    /// </summary>
    [Serializable]
    public enum DataMaskingType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// BlankOut
        /// </summary>
        BlankOut = 1,

        /// <summary>
        /// CardNumber
        /// </summary>
        CardNumber = 2,

        /// <summary>
        /// Encrypt
        /// </summary>
        Encrypt = 3,

        /// <summary>
        /// The hash
        /// </summary>
        Hash = 4
    }
}