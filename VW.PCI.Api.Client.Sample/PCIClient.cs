using System;
using VW.Api.RestClient;
using VW.PCI.Api.Client;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Infrastructure;
using VW.PCI.Api.Client.Sample.Providers;

namespace VW.PCI.Api.Client.Sample
{
    /// <summary>
    /// Concrete PCI client của consumer project.
    /// Kế thừa PCIServiceClient, wire dependencies của project này vào,
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
                tokenProvider:  new SamplePCITokenProvider(
                    new AuthTokenRequest
                    {
                        ApplicationId   = "your-application-id",
                        ApplicationName = "VisionWeb",
                        ApplicationCode = "your-application-code"
                    },
                    SampleLogger.Instance
                )
            )
        { }
    }
}
