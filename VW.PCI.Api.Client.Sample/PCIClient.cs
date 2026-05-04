using System;
using VW.PCI.Api.Client;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Infrastructure;

namespace VW.PCI.Api.Client.Sample
{
    /// <summary>
    /// Concrete PCI client của consumer project.
    /// Kế thừa PCIServiceClient, implement token storage bằng static in-memory field,
    /// và expose Singleton Instance để dùng ở bất kỳ đâu.
    /// </summary>
    public class PCIClient : PCIServiceClient
    {
        private static readonly Lazy<PCIClient> _lazyInstance =
            new Lazy<PCIClient>(() => new PCIClient());

        public static PCIClient Instance => _lazyInstance.Value;

        private PCIClient()
            : base(
                logger:         SampleLogger.Instance,
                loggingService: SampleLoggingService.Instance,
                credentials:    new AuthTokenRequest
                {
                    ApplicationId   = "your-application-id",
                    ApplicationName = "VisionWeb",
                    ApplicationCode = "your-application-code"
                }
            )
        { }

        private static PCITokenInfo _cachedToken;
        protected override PCITokenInfo GetTokenFromDB() => _cachedToken;
        protected override void SaveTokenToDB(PCITokenInfo token) => _cachedToken = token;
    }
}
