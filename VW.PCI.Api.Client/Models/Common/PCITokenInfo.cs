using System;

namespace VW.PCI.Api.Client.Models.Common
{
    public class PCITokenInfo
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public DateTime? ExpireAt { get; set; }

        public bool IsValid() =>
            !string.IsNullOrWhiteSpace(AccessToken) &&
            ExpireAt.HasValue &&
            DateTime.UtcNow < ExpireAt.Value;
    }
}
