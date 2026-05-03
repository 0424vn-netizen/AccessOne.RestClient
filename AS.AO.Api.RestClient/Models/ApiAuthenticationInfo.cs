using System;
using System.Collections.Generic;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API authentication information
    /// </summary>
    public class ApiAuthenticationInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public ApiAuthenticationMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the list of API section information.
        /// </summary>
        public IDictionary<string, ApiSettingSectionInfo> Settings { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAuthenticationInfo"/> class.
        /// </summary>
        public ApiAuthenticationInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiAuthenticationInfo"/> class.
        /// </summary>
        /// <param name="authenticationInfo">The authentication information.</param>
        public ApiAuthenticationInfo(ApiAuthenticationInfo authenticationInfo)
        {
            this.Mode = authenticationInfo.Mode;

            if (authenticationInfo.Settings != null && authenticationInfo.Settings.Count > 0)
            {
                this.Settings = new Dictionary<string, ApiSettingSectionInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var setting in authenticationInfo.Settings)
                {
                    this.Settings[setting.Key] = new ApiSettingSectionInfo(setting.Value);
                }
            }
        }

        #endregion

    }
}