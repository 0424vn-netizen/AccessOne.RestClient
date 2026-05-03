using System;
using System.Collections.Generic;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API setting section
    /// </summary>
    public class ApiSettingSection
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [inherit default setting].
        /// </summary>
        public bool InheritDefaultSetting { get; set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        public IDictionary<string, ApiSettingSectionInfo> Settings { get; set; }

        /// <summary>
        /// Gets or sets the sub sections.
        /// </summary>
        public IDictionary<string, ApiSettingSection> SubSections { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSettingSection"/> class.
        /// </summary>
        public ApiSettingSection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiSettingSection"/> class.
        /// </summary>
        /// <param name="section">The section.</param>
        public ApiSettingSection(ApiSettingSection section)
        {
            this.Name = section.Name;
            this.SubSections = section.SubSections;
            this.InheritDefaultSetting = section.InheritDefaultSetting;

            if (section.Settings != null && section.Settings.Count > 0)
            {
                this.Settings = new Dictionary<string, ApiSettingSectionInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var setting in section.Settings)
                {
                    this.Settings[setting.Key] = new ApiSettingSectionInfo(setting.Value);
                }
            }

            if (section.SubSections != null && section.SubSections.Count > 0)
            {
                this.SubSections = new Dictionary<string, ApiSettingSection>(StringComparer.OrdinalIgnoreCase);
                foreach (var subSection in section.SubSections)
                {
                    this.SubSections[subSection.Key] = new ApiSettingSection(subSection.Value);
                }
            }
        }

        #endregion

    }
}