using VW.Api.RestClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace VW.Api.RestClient
{
    /// <summary>
    /// The Api Settings Manager.
    /// </summary>
    public static class ApiSettingsManager
    {
        #region Private members

        /// <summary>
        /// The setting files directory
        /// </summary>
        private static string _settingFilesDirectory = null;

        /// <summary>
        /// The client certificate files directory
        /// </summary>
        private static string _clientCertificateFilesDirectory = null;

        /// <summary>
        /// The mock files directory
        /// </summary>
        private static string _mockFileFilesDirectory = null;

        #endregion

        #region Public methods

        /// <summary>
        /// Setups the Api setting.
        /// </summary>
        /// <param name="settingFilesDirectory">The setting files directory.</param>
        /// <param name="clientCertificateFilesDirectory">The client certificate files directory.</param>
        /// <param name="mockFilesDirectory">The mock files directory.</param>
        public static void Setup(string settingFilesDirectory, string clientCertificateFilesDirectory = null, string mockFilesDirectory = null)
        {
            _settingFilesDirectory = settingFilesDirectory;
            _clientCertificateFilesDirectory = clientCertificateFilesDirectory;
            _mockFileFilesDirectory = mockFilesDirectory;
        }

        /// <summary>
        /// Gets the API setting.
        /// </summary>
        /// <param name="settingFile">The setting file.</param>
        /// <param name="source">The source.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static ApiSetting GetApiSetting(string settingFile, string source, string request)
        {
            var settings = GetApiSettings(settingFile);
            var setting = settings.FirstOrDefault(f => f.Source.Equals(source, StringComparison.OrdinalIgnoreCase) &&
                                                        f.Name.Equals(request, StringComparison.OrdinalIgnoreCase));
            if (setting == null)
            {
                var defaultSetting = settings.FirstOrDefault(f => f.Source.Equals(source, StringComparison.OrdinalIgnoreCase) &&
                                                        f.Name.Equals("default", StringComparison.OrdinalIgnoreCase));
                setting = new ApiSetting
                {
                    AuthenticationInfo = defaultSetting.AuthenticationInfo,
                    ProxySetting = defaultSetting.ProxySetting,
                    BaseUrl = defaultSetting.BaseUrl,
                    MockFile = defaultSetting.MockFile,
                    ClientCertificateFile = defaultSetting.ClientCertificateFile,
                    ClientCertificatePassword = defaultSetting.ClientCertificatePassword,
                    RequestDataFormat = defaultSetting.RequestDataFormat,
                    MessageMappings = defaultSetting.MessageMappings,
                    Name = request,
                    Path = request,
                    IsDisabled = defaultSetting.IsDisabled,
                    Sections = defaultSetting.Sections,
                    Source = defaultSetting.Source,
                    Ssl = defaultSetting.Ssl,
                    BypassCertVerification = defaultSetting.BypassCertVerification,
                    DefaultRemapMessageCode = defaultSetting.DefaultRemapMessageCode,
                    JsonSerializerProvider = defaultSetting.JsonSerializerProvider,
                    IncludeRequestHeadersInTrackingLog = defaultSetting.IncludeRequestHeadersInTrackingLog,
                    IncludeResponseHeadersInTrackingLog = defaultSetting.IncludeResponseHeadersInTrackingLog
                };

                settings.Add(setting);
            }
            else if (string.IsNullOrWhiteSpace(setting.Path))
            {
                setting.Path = setting.Name;
            }

            return setting;
        }

        /// <summary>
        /// Gets the request settings.
        /// </summary>
        /// <param name="settingFile">The setting file.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">settingFile</exception>
        /// <exception cref="FileNotFoundException">Could not find the setting file</exception>
        public static List<ApiSetting> GetApiSettings(string settingFile)
        {
            var cacheKey = $"ApiSettingCacheKey_{Path.GetFileNameWithoutExtension(settingFile)}";
            var settings = FileCachingManager.Get<List<ApiSetting>>(cacheKey);
            if (settings != null)
            {
                return settings;
            }

            if (string.IsNullOrWhiteSpace(settingFile))
            {
                throw new ArgumentNullException(nameof(settingFile));
            }

            settingFile = Path.Combine(_settingFilesDirectory, settingFile);
            if (!File.Exists(settingFile))
            {
                throw new FileNotFoundException("Could not find the setting file", settingFile);
            }

            settings = LoadSettingsFromFile(settingFile);
            FileCachingManager.Set(cacheKey, settings, settingFile);

            return settings;
        }

        /// <summary>
        /// Gets the name of the mock file.
        /// </summary>
        /// <param name="mockFile">The mock file.</param>
        /// <returns>System.String.</returns>
        public static string GetMockFileName(string mockFile)
        {
            if (string.IsNullOrWhiteSpace(mockFile) ||
                string.IsNullOrWhiteSpace(_mockFileFilesDirectory) ||
                !mockFile.StartsWith(_mockFileFilesDirectory))
            {
                return mockFile;
            }

            return mockFile.Substring(_mockFileFilesDirectory.Length).TrimStart(new[] { '/', '\\' });
        }

        /// <summary>
        /// Gets the name of the client certificate file.
        /// </summary>
        /// <param name="clientCertificateFile">The client certificate file.</param>
        /// <returns>System.String.</returns>
        public static string GetClientCertificateFileName(string clientCertificateFile)
        {
            if (string.IsNullOrWhiteSpace(clientCertificateFile) ||
                string.IsNullOrWhiteSpace(_clientCertificateFilesDirectory) ||
                !clientCertificateFile.StartsWith(_clientCertificateFilesDirectory))
            {
                return clientCertificateFile;
            }

            return clientCertificateFile.Substring(_mockFileFilesDirectory.Length).TrimStart(new[] { '/', '\\' });
        }

        #endregion

        #region Protected/Private methods

        /// <summary>
        /// Loads the settings from file.
        /// </summary>
        /// <param name="settingFile">The setting file.</param>
        /// <returns></returns>
        private static List<ApiSetting> LoadSettingsFromFile(string settingFile)
        {
            var settings = new List<ApiSetting>();
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(settingFile);

            var nodes = xmlDocument.SelectNodes("//api");
            if (nodes == null || nodes.Count == 0)
            {
                return settings;
            }

            #region Load settings for API request

            foreach (XmlNode node in nodes)
            {
                bool.TryParse(node.Attributes.GetTrimmedValueOrDefault("inheritDefaultSetting", "true"), out var inheritDefaultSetting);

                bool? sslEnabled = null;
                var sslValue = node.Attributes.GetTrimmedValueOrDefault("ssl");
                if (!string.IsNullOrEmpty(sslValue) && bool.TryParse(sslValue, out var isSsl))
                {
                    sslEnabled = isSsl;
                }

                bool? bypassCertVerification = null;
                var bypassCertVerificationValue = node.Attributes.GetTrimmedValueOrDefault("bypassCertVerification");
                if (!string.IsNullOrEmpty(bypassCertVerificationValue) && bool.TryParse(bypassCertVerificationValue, out var isBypassCertVerification))
                {
                    bypassCertVerification = isBypassCertVerification;
                }

                var requestDataFormat = node.Attributes.GetTrimmedValueOrDefault("requestDataFormat", "json");
                ApiDataFormat apiDataFormat;
                switch (requestDataFormat.ToLower())
                {
                    case "none":
                        apiDataFormat = ApiDataFormat.None;
                        break;

                    case "xml":
                        apiDataFormat = ApiDataFormat.Xml;
                        break;

                    case "formurlencoded":
                        apiDataFormat = ApiDataFormat.FormUrlEncoded;
                        break;

                    default:
                        apiDataFormat = ApiDataFormat.Json;
                        break;
                }

                JsonSerializerProvider? jsonSerializerProvider = null;
                var serializer = node.Attributes.GetTrimmedValueOrDefault("serializer");

                if (!string.IsNullOrWhiteSpace(serializer))
                {
                    switch (serializer.ToLower())
                    {
                        case "newtonsoft":
                            jsonSerializerProvider = JsonSerializerProvider.Newtonsoft;
                            break;

                        case "default":
                            jsonSerializerProvider = JsonSerializerProvider.Default;
                            break;
                    }
                }

                var name = node.Attributes.GetTrimmedValueOrDefault("name");
                var isDefaultSetting = "default".Equals(name, StringComparison.OrdinalIgnoreCase);

                bool? isDisabled = null;
                var isDisabledSetting = node.Attributes.GetTrimmedValueOrDefault("disabled");
                if (!string.IsNullOrEmpty(isDisabledSetting) && bool.TryParse(isDisabledSetting, out var disabled))
                {
                    isDisabled = disabled;
                }

                bool? includeRequestHeadersInTrackingLog = null;
                var logRequestHeadersSetting = node.Attributes.GetTrimmedValueOrDefault("includeRequestHeadersInTrackingLog");
                if (!string.IsNullOrEmpty(logRequestHeadersSetting) && bool.TryParse(logRequestHeadersSetting, out var logRequestHeaders))
                {
                    includeRequestHeadersInTrackingLog = logRequestHeaders;
                }

                bool? includeResponseHeadersInTrackingLog = null;
                var logResponseHeadersSetting = node.Attributes.GetTrimmedValueOrDefault("includeResponseHeadersInTrackingLog");
                if (!string.IsNullOrEmpty(logResponseHeadersSetting) && bool.TryParse(logResponseHeadersSetting, out var logResponseHeaders))
                {
                    includeResponseHeadersInTrackingLog = logResponseHeaders;
                }

                var mockFile = node.SelectSingleNode("mockFile")?.Attributes.GetTrimmedValueOrDefault("value")?.TrimEnd(new[] { '/', '\\' });
                if (!string.IsNullOrEmpty(mockFile) && !string.IsNullOrWhiteSpace(_mockFileFilesDirectory))
                {
                    mockFile = Path.Combine(_mockFileFilesDirectory, mockFile);
                }

                string clientCertificateFile = null;
                string clientCertificatePassword = null;

                var clientCertificateNode = node.SelectSingleNode("clientCertificate");
                if (clientCertificateNode != null && clientCertificateNode.Attributes != null)
                {
                    clientCertificateFile = clientCertificateNode.Attributes.GetTrimmedValueOrDefault("file")?.TrimEnd(new[] { '/', '\\' });
                    if (!string.IsNullOrEmpty(clientCertificateFile) && !string.IsNullOrWhiteSpace(_clientCertificateFilesDirectory))
                    {
                        clientCertificateFile = Path.Combine(_clientCertificateFilesDirectory, clientCertificateFile);
                    }

                    clientCertificatePassword = clientCertificateNode.Attributes.GetTrimmedValueOrDefault("password");
                    var encrypted = clientCertificateNode.Attributes.GetValueOrDefault("encrypted");

                    if (!string.IsNullOrWhiteSpace(clientCertificatePassword) && ("1".Equals(encrypted) || "true".Equals(encrypted, StringComparison.OrdinalIgnoreCase)))
                    {
                        var encryptKey = clientCertificateNode.Attributes.GetTrimmedValueOrDefault("encryptKey", string.Empty);
                        clientCertificatePassword = encryptKey.Length == 0 ? CryptoUtils.Decrypt(clientCertificatePassword) : CryptoUtils.Decrypt(clientCertificatePassword, encryptKey);
                    }
                }

                MockService mockService = BuidlMockService(node);

                string reWriteUrl = node.Attributes.GetTrimmedValueOrDefault("reWriteUrl")?.Trim();

                var setting = new ApiSetting
                {
                    Name = name,
                    Source = node.Attributes.GetTrimmedValueOrDefault("source"),
                    BaseUrl = node.SelectSingleNode("baseUrl")?.Attributes.GetTrimmedValueOrDefault("value")?.TrimEnd(new[] { '/', '\\' }),
                    MockFile = mockFile,
                    ClientCertificateFile = clientCertificateFile,
                    ClientCertificatePassword = clientCertificatePassword,
                    Path = node.Attributes.GetTrimmedValueOrDefault("path", name),
                    IsDisabled = isDisabled,
                    RequestDataFormat = apiDataFormat,
                    AuthenticationInfo = GetAuthenticationInfo(node),
                    MessageMappings = GetApiMessageMappingInfos(node),
                    ProxySetting = GetProxySetting(node),
                    Sections = GetApiSection(node),
                    IsDefaultSetting = isDefaultSetting,
                    InheritDefaultSetting = inheritDefaultSetting,
                    Ssl = sslEnabled,
                    BypassCertVerification = bypassCertVerification,
                    MessageSources = node.Attributes.GetTrimmedValueOrDefault("inheritMessageSources", string.Empty).Split(new[] { ',' }).ToList(),
                    JsonSerializerProvider = jsonSerializerProvider,
                    IncludeRequestHeadersInTrackingLog = includeRequestHeadersInTrackingLog,
                    IncludeResponseHeadersInTrackingLog = includeResponseHeadersInTrackingLog,
                    MockService = mockService,
                    ReWriteUrl = reWriteUrl
                };

                settings.Add(setting);
            }

            #endregion

            // Synchronize settings from default setting
            return SyncSettingsFromDefault(settings);
        }

        private static MockService BuidlMockService(XmlNode node)
        {
           
            if (node != null)
            {
                MockService mockService = new MockService();

                XmlNode mockServiceNode = node.SelectSingleNode("mockService");
                if (mockServiceNode != null && mockServiceNode.Attributes != null)
                {
                    mockService.Url = mockServiceNode.Attributes.GetTrimmedValueOrDefault("url");
                    mockService.Environment = mockServiceNode.Attributes.GetTrimmedValueOrDefault("environment");
                    mockService.Module = mockServiceNode.Attributes.GetTrimmedValueOrDefault("module");
                    mockService.Source = mockServiceNode.Attributes.GetTrimmedValueOrDefault("source");
                    mockService.Version = mockServiceNode.Attributes.GetTrimmedValueOrDefault("version");

                    string enabledValue = mockServiceNode.Attributes.GetTrimmedValueOrDefault("enabled");
                    if (!string.IsNullOrEmpty(enabledValue))
                    {
                        bool Enabled;
                        bool.TryParse(enabledValue, out Enabled);
                        mockService.Enabled = Enabled;
                    }
                    
                }
                return mockService;
            }
            return default;
        }

        /// <summary>
        /// Synchronizes the settings from default.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        private static List<ApiSetting> SyncSettingsFromDefault(List<ApiSetting> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return settings;
            }

            var defaultSettings = settings.FindAll(s => s.IsDefaultSetting);
            foreach (var defaultSetting in defaultSettings)
            {
                var settingsBySource = settings.FindAll(s => !s.IsDefaultSetting && s.InheritDefaultSetting && s.Source.Equals(defaultSetting.Source, StringComparison.OrdinalIgnoreCase));
                if (settingsBySource.Count == 0)
                {
                    continue;
                }

                foreach (var apiSetting in settingsBySource)
                {
                    if (string.IsNullOrEmpty(apiSetting.BaseUrl) && !string.IsNullOrEmpty(defaultSetting.BaseUrl))
                    {
                        apiSetting.BaseUrl = defaultSetting.BaseUrl;
                    }

                    if (string.IsNullOrEmpty(apiSetting.MockFile) && !string.IsNullOrEmpty(defaultSetting.MockFile))
                    {
                        apiSetting.MockFile = defaultSetting.MockFile;
                    }

                    if (string.IsNullOrEmpty(apiSetting.ClientCertificateFile) && !string.IsNullOrEmpty(defaultSetting.ClientCertificateFile))
                    {
                        apiSetting.ClientCertificateFile = defaultSetting.ClientCertificateFile;
                    }

                    if (string.IsNullOrEmpty(apiSetting.ClientCertificatePassword) && !string.IsNullOrEmpty(defaultSetting.ClientCertificatePassword))
                    {
                        apiSetting.ClientCertificatePassword = defaultSetting.ClientCertificatePassword;
                    }

                    if (string.IsNullOrEmpty(apiSetting.Path) && !string.IsNullOrEmpty(defaultSetting.Path))
                    {
                        apiSetting.Path = defaultSetting.Path;
                    }

                    if (!apiSetting.IsDisabled.HasValue && defaultSetting.IsDisabled.HasValue)
                    {
                        apiSetting.IsDisabled = defaultSetting.IsDisabled;
                    }

                    if (!apiSetting.Ssl.HasValue && defaultSetting.Ssl.HasValue)
                    {
                        apiSetting.Ssl = defaultSetting.Ssl;
                    }

                    if (!apiSetting.BypassCertVerification.HasValue && defaultSetting.BypassCertVerification.HasValue)
                    {
                        apiSetting.BypassCertVerification = defaultSetting.BypassCertVerification;
                    }

                    if (apiSetting.AuthenticationInfo == null && defaultSetting.AuthenticationInfo != null)
                    {
                        apiSetting.AuthenticationInfo = defaultSetting.AuthenticationInfo;
                    }

                    if (apiSetting.ProxySetting == null && defaultSetting.ProxySetting != null)
                    {
                        apiSetting.ProxySetting = defaultSetting.ProxySetting;
                    }

                    if (!apiSetting.IncludeRequestHeadersInTrackingLog.HasValue && defaultSetting.IncludeRequestHeadersInTrackingLog.HasValue)
                    {
                        apiSetting.IncludeRequestHeadersInTrackingLog = defaultSetting.IncludeRequestHeadersInTrackingLog;
                    }

                    if (!apiSetting.IncludeResponseHeadersInTrackingLog.HasValue && defaultSetting.IncludeResponseHeadersInTrackingLog.HasValue)
                    {
                        apiSetting.IncludeResponseHeadersInTrackingLog = defaultSetting.IncludeResponseHeadersInTrackingLog;
                    }

                    if (apiSetting.JsonSerializerProvider == null && defaultSetting.JsonSerializerProvider != null)
                    {
                        apiSetting.JsonSerializerProvider = defaultSetting.JsonSerializerProvider;
                    }

                    if (apiSetting.MockService == null && defaultSetting.MockService != null)
                    {
                        apiSetting.MockService = defaultSetting.MockService;
                    }
                    else if (apiSetting.MockService != null && defaultSetting.MockService != null)
                    {
                        IEnumerable<PropertyInfo> propertyInfos = typeof(MockService).GetProperties().Where(p=>p.CanWrite && p.PropertyType != typeof(bool));
                        foreach (PropertyInfo prop in propertyInfos)
                        {
                            var apiValue = prop.GetValue(apiSetting.MockService);
                            var defaultValue = prop.GetValue(defaultSetting.MockService);
                            if(string.IsNullOrEmpty(apiValue?.ToString()) && !string.IsNullOrEmpty(defaultValue?.ToString()))
                            {
                                prop.SetValue(apiSetting.MockService, defaultValue);
                            }
                        }
                    }

                    //finally setup for mockservice
                    if (apiSetting.MockService != null && defaultSetting.MockService != null)
                    {
                        if(!apiSetting.MockService.Enabled.HasValue)
                            apiSetting.MockService.Enabled = defaultSetting.MockService.Enabled;
                        if(string.IsNullOrEmpty(apiSetting.MockService.Name))
                            apiSetting.MockService.Name = apiSetting.Name;
                    }

                    if (defaultSetting.MessageMappings != null && defaultSetting.MessageMappings.Count > 0)
                    {
                        var messageMappings = defaultSetting.MessageMappings;
                        var messageSources = apiSetting.MessageSources;
                        if (messageSources != null && messageSources.Count > 0)
                        {
                            messageMappings = new List<ApiMessageMappingInfo>();

                            var defaultMessageMappings = defaultSetting.MessageMappings.FindAll(m => string.IsNullOrWhiteSpace(m.Source)) ?? new List<ApiMessageMappingInfo>();
                            if (defaultMessageMappings != null && defaultMessageMappings.Count > 0)
                            {
                                messageMappings.AddRange(defaultMessageMappings);
                            }

                            foreach (var messageSource in messageSources)
                            {
                                var messageSourceMappings = defaultSetting.MessageMappings.FindAll(m => messageSource.Equals(m.Source, StringComparison.OrdinalIgnoreCase));
                                if (messageSourceMappings != null && messageSourceMappings.Count > 0)
                                {
                                    messageMappings.AddRange(messageSourceMappings);
                                }
                            }
                        }

                        if (apiSetting.MessageMappings == null)
                        {
                            apiSetting.MessageMappings = messageMappings;
                        }
                        else
                        {
                            foreach (var item in messageMappings)
                            {
                                if (apiSetting.MessageMappings.Any(m => m.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase) &&
                                                                        m.Type.Equals(item.Type, StringComparison.OrdinalIgnoreCase) &&
                                                                        m.Description.Equals(item.Description, StringComparison.OrdinalIgnoreCase)))
                                {
                                    continue;
                                }

                                apiSetting.MessageMappings.Add(item);
                            }
                        }
                    }

                    if (defaultSetting.Sections != null)
                    {
                        if (apiSetting.Sections == null)
                        {
                            apiSetting.Sections = new Dictionary<string, ApiSettingSection>(StringComparer.OrdinalIgnoreCase);
                        }

                        var sections = apiSetting.Sections;
                        var defaultSections = defaultSetting.Sections;

                        foreach (var defaultSection in defaultSections)
                        {
                            if (!sections.ContainsKey(defaultSection.Key))
                            {
                                sections[defaultSection.Key] = new ApiSettingSection(defaultSections[defaultSection.Key]);
                                continue;
                            }

                            if (!sections[defaultSection.Key].InheritDefaultSetting)
                            {
                                continue;
                            }

                            var sectionInfos = sections[defaultSection.Key].Settings;
                            var defaultSectionInfos = defaultSections[defaultSection.Key].Settings;
                            foreach (var setting in defaultSectionInfos)
                            {
                                if (sectionInfos.ContainsKey(setting.Key))
                                {
                                    continue;
                                }

                                sectionInfos[setting.Key] = defaultSectionInfos[setting.Key];
                            }
                        }
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Gets the authentication information.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static ApiAuthenticationInfo GetAuthenticationInfo(XmlNode node)
        {
            var authNode = node?.SelectSingleNode("authentication");
            if (authNode == null)
            {
                return null;
            }

            ApiAuthenticationMode mode;
            var settingMode = authNode?.Attributes?.GetTrimmedValueOrDefault("mode") ?? string.Empty;
            switch (settingMode.ToLower())
            {
                case "custom":
                    mode = ApiAuthenticationMode.Custom;
                    break;

                case "basic":
                    mode = ApiAuthenticationMode.Basic;
                    break;

                case "jwt":
                    mode = ApiAuthenticationMode.Jwt;
                    break;

                case "simple":
                    mode = ApiAuthenticationMode.Simple;
                    break;

                case "ntml":
                    mode = ApiAuthenticationMode.Ntml;
                    break;

                case "oauth2":
                    mode = ApiAuthenticationMode.OAuth2;
                    break;

                case "oauth2uriqueryparameter":
                    mode = ApiAuthenticationMode.OAuth2UriQueryParameter;
                    break;

                default:
                    mode = ApiAuthenticationMode.None;
                    break;
            }

            var settingSections = GetSettingSections(authNode);
            return new ApiAuthenticationInfo
            {
                Mode = mode,
                Settings = settingSections
            };
        }

        /// <summary>
        /// Gets the proxy information.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static ApiProxyInfo GetProxySetting(XmlNode node)
        {
            var proxyNode = node?.SelectSingleNode("proxy");
            if (proxyNode == null)
            {
                return null;
            }

            var enable = proxyNode?.Attributes?.GetTrimmedValueOrDefault("enable");
            var proxySetting = new ApiProxyInfo
            {
                IsEnable = "true".Equals(enable, StringComparison.OrdinalIgnoreCase),
            };

            var bypassListNodes = proxyNode.SelectSingleNode("section[@name='bypassList']")?.ChildNodes;
            if (bypassListNodes != null)
            {
                var bypassLists = new List<string>();
                foreach (XmlNode bypassListNode in bypassListNodes)
                {
                    var bypassItem = bypassListNode.Attributes.GetTrimmedValueOrDefault("value");
                    if (!string.IsNullOrEmpty(bypassItem))
                    {
                        bypassLists.Add(bypassItem);
                    }
                }

                if (bypassLists.Count > 0)
                {
                    proxySetting.BypassList = bypassLists.ToArray();
                }
            }

            var addressNode = proxyNode.SelectSingleNode("add[@name='address']");
            proxySetting.Address = addressNode?.Attributes.GetTrimmedValueOrDefault("value");

            var domainNode = proxyNode.SelectSingleNode("add[@name='domain']");
            proxySetting.Domain = domainNode?.Attributes.GetTrimmedValueOrDefault("value");

            var bypassOnlocalNode = proxyNode.SelectSingleNode("add[@name='bypassOnlocal']");
            var val = bypassOnlocalNode?.Attributes.GetTrimmedValueOrDefault("value");
            proxySetting.BypassOnlocal = "true".Equals(val, StringComparison.OrdinalIgnoreCase);

            var userNameNode = proxyNode.SelectSingleNode("add[@name='username']");
            if (userNameNode != null)
            {
                proxySetting.UserName = GetSettingSectionInfo(userNameNode).Value;
            }

            var passwordNode = proxyNode.SelectSingleNode("add[@name='password']");
            if (passwordNode != null)
            {
                proxySetting.Password = GetSettingSectionInfo(passwordNode).Value;
            }

            return proxySetting;
        }

        /// <summary>
        /// Gets the API message mapping infos.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static List<ApiMessageMappingInfo> GetApiMessageMappingInfos(XmlNode node)
        {
            var messageSettingsNode = node?.SelectSingleNode("messageSettings");
            if (messageSettingsNode == null)
            {
                return null;
            }

            var messageNodes = messageSettingsNode.SelectNodes("message");
            if (messageNodes == null || messageNodes.Count == 0)
            {
                return null;
            }

            var messageMappingInfos = new List<ApiMessageMappingInfo>();
            var defaultMessageCode = messageSettingsNode.Attributes.GetTrimmedValueOrDefault("defaultRemapMesssageCode", "500");

            foreach (XmlNode messageNode in messageNodes)
            {
                var attributes = messageNode.Attributes;
                var code = attributes.GetTrimmedValueOrDefault("code", string.Empty);
                var type = attributes.GetTrimmedValueOrDefault("type", string.Empty);
                var description = attributes.GetTrimmedValueOrDefault("description", string.Empty);
                var remapMessageType = attributes.GetTrimmedValueOrDefault("remapMessageType", string.Empty);
                int.TryParse(attributes.GetTrimmedValueOrDefault("remapMessageCode", defaultMessageCode), out var remapMessageCode);
                var source = attributes.GetTrimmedValueOrDefault("source", string.Empty);

                messageMappingInfos.Add(new ApiMessageMappingInfo
                {
                    Code = code,
                    Type = type,
                    Description = description,
                    RemapMessageCode = remapMessageCode,
                    RemapMessageType = remapMessageType,
                    Source = source
                });
            }

            return messageMappingInfos;
        }

        /// <summary>
        /// Gets the API section.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static IDictionary<string, ApiSettingSection> GetApiSection(XmlNode node)
        {
            var sectionNodes = node?.SelectNodes("section");
            if (sectionNodes == null || sectionNodes.Count == 0)
            {
                return null;
            }

            var sections = new Dictionary<string, ApiSettingSection>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode sectionNode in sectionNodes)
            {
                var name = sectionNode.Attributes.GetTrimmedValueOrDefault("name");
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                bool.TryParse(node.Attributes.GetTrimmedValueOrDefault("inheritDefaultSetting", "true"), out var inheritDefaultSetting);
                var section = new ApiSettingSection
                {
                    Name = name,
                    InheritDefaultSetting = inheritDefaultSetting,
                    Settings = GetSettingSections(sectionNode),
                    SubSections = GetApiSection(sectionNode)
                };

                sections[name] = section;
            }

            return sections;
        }

        /// <summary>
        /// Gets the setting sections.
        /// </summary>
        /// <param name="settingNode">The setting node.</param>
        /// <returns></returns>
        private static IDictionary<string, ApiSettingSectionInfo> GetSettingSections(XmlNode settingNode)
        {
            var childNodes = settingNode?.ChildNodes;
            if (childNodes == null || childNodes.Count == 0)
            {
                return null;
            }

            var sections = new Dictionary<string, ApiSettingSectionInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode childNode in childNodes)
            {
                var sectionInfo = GetSettingSectionInfo(childNode);
                if (!string.IsNullOrWhiteSpace(sectionInfo.Name))
                {
                    sections[sectionInfo.Name] = sectionInfo;
                }
            }

            return sections;
        }

        /// <summary>
        /// Gets the API section information.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static ApiSettingSectionInfo GetSettingSectionInfo(XmlNode node)
        {
            var attributes = node.Attributes;
            var value = attributes.GetTrimmedValueOrDefault("value");
            var encrypted = attributes.GetValueOrDefault("encrypted");

            if ("1".Equals(encrypted) || "true".Equals(encrypted, StringComparison.OrdinalIgnoreCase))
            {
                var encryptKey = attributes.GetTrimmedValueOrDefault("encryptKey", string.Empty);
                value = encryptKey.Length == 0 ? CryptoUtils.Decrypt(value) : CryptoUtils.Decrypt(value, encryptKey);
            }

            int.TryParse(attributes.GetTrimmedValueOrDefault("order"), out int order);
            var info = new ApiSettingSectionInfo
            {
                Name = attributes.GetTrimmedValueOrDefault("name"),
                Value = value,
                Order = order
            };

            return info;
        }

        #endregion
    }
}