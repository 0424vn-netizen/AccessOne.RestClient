namespace VW.Api.RestClient.Models
{
    /// <summary>
    /// The API proxy information
    /// </summary>
    public class ApiProxyInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enable.
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [bypass onlocal].
        /// </summary>
        public bool BypassOnlocal { get; set; }

        /// <summary>
        /// Gets or sets the bypass list.
        /// </summary>
        public string[] BypassList { get; set; }

        #endregion
    }
}
