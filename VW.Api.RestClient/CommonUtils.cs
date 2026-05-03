using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The common utility
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// Converts the time from UTC.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="toTimeZoneById">To time zone by identifier.</param>
        /// <returns></returns>
        public static DateTime? ConvertTimeFromUtc(DateTime? dateTime, string toTimeZoneById)
        {
            if (!dateTime.HasValue || string.IsNullOrWhiteSpace(toTimeZoneById))
            {
                return dateTime;
            }

            var toTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneById);
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value, toTimeZoneInfo);
        }

        /// <summary>
        /// Converts the time from timezone to another.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="fromTimeZoneInfo">From time zone information.</param>
        /// <param name="toTimeZoneById">To time zone by identifier.</param>
        /// <returns></returns>
        public static DateTime? ConvertTime(DateTime? dateTime, TimeZoneInfo fromTimeZoneInfo, string toTimeZoneById)
        {
            if (!dateTime.HasValue || string.IsNullOrWhiteSpace(toTimeZoneById))
            {
                return dateTime;
            }

            var toTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(toTimeZoneById);
            return TimeZoneInfo.ConvertTime(dateTime.Value, fromTimeZoneInfo, toTimeZoneInfo);
        }

        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetValueOrDefault(this XmlAttributeCollection attributes, string name, string defaultValue = null)
        {
            return attributes?[name] == null ? defaultValue : attributes[name].Value;
        }

        /// <summary>
        /// Gets the trimmed value or default.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetTrimmedValueOrDefault(this XmlAttributeCollection attributes, string name, string defaultValue = null)
        {
            return attributes?[name] == null ? defaultValue : attributes[name].Value.Trim();
        }

        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetValueOrDefault(this IDictionary<string, string> dictionary, string key, string defaultValue = null)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }

        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <typeparam name="T">The type of target value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IDictionary<string, T> dictionary, string key, T defaultValue = default(T))
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }

        /// <summary>
        /// Serializes the given value to XML string.
        /// </summary>
        /// <typeparam name="T">The type of given value.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns></returns>
        public static string Serialize<T>(T value, IDictionary<string, string> namespaces)
        {
            if (value == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings { Indent = true };
            var serializerNamespaces = new XmlSerializerNamespaces();
            serializerNamespaces.Add(string.Empty, string.Empty);

            if (namespaces != null && namespaces.Count > 0)
            {
                foreach (var pair in namespaces)
                {
                    serializerNamespaces.Add(pair.Key, pair.Value);
                }
            }

            using (var sw = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(xmlWriter, value, serializerNamespaces);
                    return sw.ToString();
                }
            }
        }

        /// <summary>
        /// Deserializes the given XML string to an object.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="xml">The XML.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml, T defaultValue = default(T))
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return defaultValue;
            }

            using (var reader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Converts to error string.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static string ToErrorString(this Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.AppendFormat("Exception of type '{0}': {1}{2}", exception.GetType().Name, exception.Message, Environment.NewLine);

            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                builder.AppendLine(exception.StackTrace);
            }

            if (exception.InnerException != null)
            {
                builder.AppendLine(exception.InnerException.ToErrorString());
            }

            return builder.ToString();
        }

    }
}