using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API setting
    /// </summary>
    public class ApiSetting
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the message setting sources.
        /// </summary>
        public List<string> MessageSources { get; set; }

        /// <summary>
        /// Gets or sets the service URL.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the mock file.
        /// </summary>
        public string MockFile { get; set; }

        /// <summary>
        /// Gets or sets the client certificate file.
        /// </summary>
        public string ClientCertificateFile { get; set; }

        /// <summary>
        /// Gets or sets the client certificate password.
        /// </summary>
        public string ClientCertificatePassword { get; set; }

        /// <summary>
        /// Gets or sets the SOAP action.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this ApiSetting is disabled.
        /// </summary>
        public bool? IsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the include request headers in api tracking log.
        /// </summary>
        public bool? IncludeRequestHeadersInTrackingLog { get; set; }

        /// <summary>
        /// Gets or sets the include response headers in api tracking log.
        /// </summary>
        public bool? IncludeResponseHeadersInTrackingLog { get; set; }

        /// <summary>
        /// Gets or sets the json serializer provider.
        /// </summary>
        public JsonSerializerProvider? JsonSerializerProvider { get; set; }

        /// <summary>
        /// Gets or sets the Request Data Format
        /// </summary>
        public ApiDataFormat RequestDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the authentication information.
        /// </summary>
        public ApiAuthenticationInfo AuthenticationInfo { get; set; }

        /// <summary>
        /// Gets or sets the API proxy information.
        /// </summary>
        public ApiProxyInfo ProxySetting { get; set; }

        /// <summary>
        /// Gets or sets the API message mapping settings.
        /// </summary>
        public List<ApiMessageMappingInfo> MessageMappings { get; set; }

        /// <summary>
        /// Gets or sets the default return message code.
        /// </summary>
        public int DefaultRemapMessageCode { get; set; } = 500;

        public MockService MockService { get; set; } = new MockService();
        public string ReWriteUrl { get; set; }
        /// <summary>
        /// Gets or sets the sections.
        /// </summary>
        public IDictionary<string, ApiSettingSection> Sections { get; set; }

        /// <summary>
        /// Gets or sets the runtime settings.
        /// </summary>
        public IDictionary<string, object> RuntimeSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should inherit settings from default.
        /// </summary>
        public bool InheritDefaultSetting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default setting.
        /// </summary>
        public bool IsDefaultSetting { get; set; }

        /// <summary>
        /// Gets or sets the SSL.
        /// </summary>
        public bool? Ssl { get; set; }

        /// <summary>
        /// Gets or sets the bypass cert verification.
        /// </summary>
        public bool? BypassCertVerification { get; set; }

        /// <summary>
        /// Remaps the message code.
        /// </summary>
        /// <param name="apiMessage">The API message.</param>
        /// <returns></returns>
        public void RemapMessage(ApiMessage apiMessage)
        {
            if (apiMessage == null)
            {
                return;
            }

            ApiMessageMappingInfo messageMapping = null;
            var code = (apiMessage.Code ?? string.Empty).Trim();
            var type = (apiMessage.Type ?? string.Empty).Trim();
            var description = (apiMessage.Description ?? string.Empty).Trim();

            var messageMappings = this.MessageMappings?.FindAll(m => m.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (messageMappings != null && messageMappings.Count > 0)
            {
                messageMapping = messageMappings.FirstOrDefault(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
                                                                        && IsMatchDescription(s.Description, description));
                if (messageMapping == null)
                {
                    messageMapping = messageMappings.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Type)
                                                                             && IsMatchDescription(s.Description, description));
                }

                if (messageMapping == null)
                {
                    messageMapping = messageMappings.FirstOrDefault(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
                                                                        && string.IsNullOrWhiteSpace(s.Description));
                }

                if (messageMapping == null)
                {
                    messageMapping = messageMappings.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Type)
                                                                        && string.IsNullOrWhiteSpace(s.Description));
                }
            }
            else
            {
                messageMappings = this.MessageMappings?.FindAll(m => string.IsNullOrWhiteSpace(m.Code)
                                                                        && IsMatchDescription(m.Description, description));
                if (messageMappings != null && messageMappings.Count > 0)
                {
                    messageMapping = messageMappings.FirstOrDefault(s => s.Type.Equals(type, StringComparison.OrdinalIgnoreCase)) ??
                                        messageMappings.FirstOrDefault(s => string.IsNullOrWhiteSpace(s.Type));
                }
            }

            if (messageMapping != null)
            {
                apiMessage.MessageCode = messageMapping.RemapMessageCode;
                apiMessage.Type = string.IsNullOrWhiteSpace(messageMapping.RemapMessageType) ? apiMessage.Type : messageMapping.RemapMessageType;
            }
            else
            {
                apiMessage.MessageCode = int.TryParse(code, out var messageCode) ? messageCode : this.DefaultRemapMessageCode;
            }
        }

        /// <summary>
        /// Gets the API setting section.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="defaultIfNull">The default if null.</param>
        /// <returns></returns>
        public ApiSettingSection GetApiSettingSection(string sectionName, ApiSettingSection defaultIfNull = null)
        {
            if (this.Sections == null || !this.Sections.ContainsKey(sectionName))
            {
                return defaultIfNull;
            }

            return this.Sections[sectionName];
        }

        /// <summary>
        /// Gets the API setting value.
        /// </summary>
        /// <param name="section">The section.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="defaultIfNull">The default if null.</param>
        /// <returns></returns>
        public string GetApiSettingValue(ApiSettingSection section, string settingKey, string defaultIfNull = null)
        {
            if (section == null || section.Settings == null || !section.Settings.ContainsKey(settingKey))
            {
                return defaultIfNull;
            }

            return section.Settings[settingKey]?.Value ?? defaultIfNull;
        }

        /// <summary>
        /// Gets the API setting value.
        /// </summary>
        /// <param name="sectionName">Name of the section.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="defaultIfNull">The default if null.</param>
        /// <returns></returns>
        public string GetApiSettingValue(string sectionName, string settingKey, string defaultIfNull = null)
        {
            var section = GetApiSettingSection(sectionName);
            if (section == null || section.Settings == null || !section.Settings.ContainsKey(settingKey))
            {
                return defaultIfNull;
            }

            return section.Settings[settingKey]?.Value ?? defaultIfNull;
        }

        /// <summary>
        /// Checks if the message description is matched with input pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="description">The description.</param>
        private bool IsMatchDescription(string pattern, string description)
        {
            if (description.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(description);
        }
    }
}