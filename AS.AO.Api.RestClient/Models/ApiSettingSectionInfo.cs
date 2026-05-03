using System;

namespace AS.AO.Api.RestClient.Models
{
    /// <summary>
    /// The API setting section information
    /// </summary>
    public class ApiSettingSectionInfo : IComparable<ApiSettingSectionInfo>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(ApiSettingSectionInfo other)
        {
            if (other == null)
            {
                return 1;
            }

            if (!this.Order.HasValue)
            {
                return -1;
            }

            return this.Order.Value.CompareTo(other.Order);
        }

        public ApiSettingSectionInfo()
        {

        }

        public ApiSettingSectionInfo(ApiSettingSectionInfo sectionInfo)
        {
            this.Name = sectionInfo.Name;
            this.Value = sectionInfo.Value;
            this.Order = sectionInfo.Order;
        }

    }
}