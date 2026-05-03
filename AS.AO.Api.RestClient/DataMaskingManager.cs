using AS.AO.Api.RestClient.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace AS.AO.Api.RestClient
{
    /// <summary>
    /// The data masking manager
    /// </summary>
    public static class DataMaskingManager
    {

        #region Variables

        /// <summary>
        /// The data masking settings cache key
        /// </summary>
        private const string CacheKey = "ApiDataMaskingManagerCacheKey";

        /// <summary>
        /// The settings file
        /// </summary>
        private static string _settingsFile = null;

        /// <summary>
        /// The logger
        /// </summary>
        private static ILogger _logger = null;

        #endregion

        #region Public methods

        /// <summary>
        /// Setups.
        /// </summary>
        /// <param name="settingsFile">The settings file.</param>
        public static void Setup(string settingsFile, ILogger logger)
        {
            _settingsFile = settingsFile;
            _logger = logger;
        }

        /// <summary>
        /// Masks the json data.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string MaskJsonData(string fieldName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var settings = GetSettings();
            if (settings == null || settings.Properties.Count == 0)
            {
                return value;
            }

            var dataMaskingType = FindMaskingType(settings.Properties, fieldName);
            return (dataMaskingType == DataMaskingType.Unknown) ? value : MaskData(dataMaskingType, value);
        }

        /// <summary>
        /// Masks the json data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string MaskJsonString(string data, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            IDictionary<string, DataMaskingType> patterns = null;

            try
            {
                var settings = GetSettings();
                patterns = settings?.Patterns;

                if (settings == null || settings.Properties == null || settings.Properties.Count == 0)
                {
                    return data;
                }

                var jToken = JToken.Parse(data);
                MaskJsonData(settings.Properties, jToken);

                var serializedString = JsonSerializer.Serialize(jToken, formatting);
                return MaskSensitiveByPattern(serializedString, patterns);
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);
            }

            return MaskSensitiveByPattern(JsonSerializer.Serialize(data, formatting), patterns);
        }

        /// <summary>
        /// Masks the json data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string MaskJsonData<T>(T data, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None)
        {
            if (data == null)
            {
                return null;
            }

            IDictionary<string, DataMaskingType> patterns = null;

            try
            {
                var settings = GetSettings();
                patterns = settings?.Patterns;

                if (settings == null || settings.Properties == null || settings.Properties.Count == 0)
                {
                    return MaskSensitiveByPattern(JsonSerializer.Serialize(data, formatting), patterns);
                }

                var jToken = JToken.FromObject(data);
                MaskJsonData(settings.Properties, jToken);

                return MaskSensitiveByPattern(JsonSerializer.Serialize(jToken, formatting), patterns);
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);
            }

            return MaskSensitiveByPattern(JsonSerializer.Serialize(data, formatting), patterns);
        }

        /// <summary>
        /// Masks the json data object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="formatting">The formatting.</param>
        /// <returns></returns>
        public static string MaskJsonDataObject(object data, Newtonsoft.Json.Formatting formatting = Newtonsoft.Json.Formatting.None)
        {
            if (data == null)
            {
                return null;
            }

            IDictionary<string, DataMaskingType> patterns = null;

            try
            {
                var settings = GetSettings();
                patterns = settings?.Patterns;

                if (settings == null || settings.Properties == null || settings.Properties.Count == 0)
                {
                    return MaskSensitiveByPattern(JsonSerializer.SerializeObject(data, formatting), patterns);
                }

                var jToken = data is string ? JToken.Parse(data as string) : JToken.FromObject(data);
                MaskJsonData(settings.Properties, jToken);

                return MaskSensitiveByPattern(JsonSerializer.SerializeObject(jToken, formatting), patterns);
            }
            catch (Exception exception)
            {
                _logger?.Error(exception);
            }

            return MaskSensitiveByPattern(JsonSerializer.SerializeObject(data, formatting), patterns);
        }

        /// <summary>
        /// Masks the json data.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="jToken">The j token.</param>
        private static void MaskJsonData(IDictionary<string, DataMaskingType> settings, JToken jToken)
        {
            if (jToken == null)
            {
                return;
            }

            if (!jToken.HasValues)
            {
                MaskJTokenValue(settings, jToken);
                return;
            }

            var element = jToken.First;
            do
            {
                MaskJTokenValue(settings, element);

                if (element.HasValues)
                {
                    MaskJsonData(settings, element);
                }
                element = element.Next;
            } while (element != null);
        }

        /// <summary>
        /// Masks the j token value.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="jToken">The j token.</param>
        private static void MaskJTokenValue(IDictionary<string, DataMaskingType> settings, JToken jToken)
        {
            if (jToken == null)
            {
                return;
            }

            switch (jToken.Type)
            {
                case JTokenType.Object:
                case JTokenType.Array:
                    if (jToken.HasValues)
                    {
                        MaskJsonData(settings, jToken);
                    }

                    break;

                case JTokenType.Property:
                    var property = jToken as JProperty;
                    if (property?.Value != null)
                    {
                        var dataMaskingType = FindMaskingType(settings, property.Name);
                        if (dataMaskingType != DataMaskingType.Unknown)
                        {
                            property.Value = MaskData(dataMaskingType, property.Value.ToString());
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Finds the type of the masking.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static DataMaskingType FindMaskingType(IDictionary<string, DataMaskingType> settings, string propertyName)
        {
            return settings.ContainsKey(propertyName) ? settings[propertyName] : DataMaskingType.Unknown;
        }

        /// <summary>
        /// Masks the input data.
        /// </summary>
        /// <param name="type">The data masking type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private static string MaskData(DataMaskingType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            switch (type)
            {
                case DataMaskingType.BlankOut:
                    return "[Blanked Out]";

                case DataMaskingType.Encrypt:
                    return CryptoUtils.TryEncrypt(value);

                case DataMaskingType.Hash:
                    return CryptoUtils.TrySHA2(value);

                case DataMaskingType.CardNumber:
                    {
                        value = value.Trim();
                        if (value.Length <= 10)
                        {
                            return new string('x', value.Length);
                        }

                        var firstSixChars = value.Substring(0, 6);
                        var lastFourChars = value.Substring(value.Length - 4);
                        var maskerChars = new string('x', 5);

                        return $"{firstSixChars}{maskerChars}{lastFourChars}";
                    }

                default:
                    return value;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets the data masking settings.
        /// </summary>
        /// <returns></returns>
        private static DataMaskingSetting GetSettings()
        {
            var settings = FileCachingManager.Get<DataMaskingSetting>(CacheKey);
            if (settings == null)
            {
                settings = LoadSettingsFromFile(_settingsFile);
                FileCachingManager.Set(CacheKey, settings, _settingsFile);
            }

            return settings;
        }
        /// <summary>
        /// Loads the settings from file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private static DataMaskingSetting LoadSettingsFromFile(string filePath)
        {
            var dataMaskingSetting = new DataMaskingSetting
            {
                Patterns = new Dictionary<string, DataMaskingType>(StringComparer.OrdinalIgnoreCase),
                Properties = new Dictionary<string, DataMaskingType>(StringComparer.OrdinalIgnoreCase)
            };

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            var nodes = xmlDocument.SelectNodes("//element");
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlNode node in nodes)
                {
                    var dataMaskingType = DataMaskingType.Unknown;
                    if (node.ParentNode != null && "maskingOption".Equals(node.ParentNode.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var type = node.ParentNode.Attributes.GetTrimmedValueOrDefault("type", string.Empty);
                        dataMaskingType = GetDataMaskingType(type);
                    }

                    var attributes = node.Attributes;
                    var name = attributes.GetTrimmedValueOrDefault("name");
                    var property = attributes.GetTrimmedValueOrDefault("property");
                    if (string.IsNullOrWhiteSpace(property) || dataMaskingType == DataMaskingType.Unknown)
                    {
                        property = name;
                    }

                    dataMaskingSetting.Properties[property] = dataMaskingType;
                }
            }

            var patternNodes = xmlDocument.SelectNodes("//pattern");
            if (patternNodes != null && patternNodes.Count > 0)
            {
                foreach (XmlNode patternNode in patternNodes)
                {
                    var attributes = patternNode.Attributes;
                    var type = attributes.GetTrimmedValueOrDefault("type");
                    var value = attributes.GetTrimmedValueOrDefault("value");
                    if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(type))
                    {
                        continue;
                    }

                    var dataMaskingType = GetDataMaskingType(type);
                    dataMaskingSetting.Patterns[value] = dataMaskingType;
                }
            }

            return dataMaskingSetting;
        }

        /// <summary>
        /// Masks the sensitive by pattern.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="patterns">The patterns.</param>
        /// <returns></returns>
        private static string MaskSensitiveByPattern(string content, IDictionary<string, DataMaskingType> patterns)
        {
            if (string.IsNullOrWhiteSpace(content) || patterns == null || patterns.Count == 0)
            {
                return content;
            }

            foreach (var pattern in patterns)
            {
                var matchs = Regex.Matches(content, pattern.Key);

                if (matchs == null || matchs.Count == 0)
                {
                    continue;
                }

                foreach (var match in matchs)
                {
                    var value = match.ToString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        continue;
                    }

                    content = content.Replace(value, MaskData(pattern.Value, value));
                }
            }

            return content;
        }

        /// <summary>
        /// Gets the type of the data masking.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static DataMaskingType GetDataMaskingType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return DataMaskingType.Unknown;
            }

            switch (type.ToLower())
            {
                case "blankout":
                    return DataMaskingType.BlankOut;

                case "cardnumber":
                    return DataMaskingType.CardNumber;

                case "encrypt":
                    return DataMaskingType.Encrypt;

                case "hash":
                    return DataMaskingType.Hash;
            }

            return DataMaskingType.Unknown;
        }

        #endregion

    }
}