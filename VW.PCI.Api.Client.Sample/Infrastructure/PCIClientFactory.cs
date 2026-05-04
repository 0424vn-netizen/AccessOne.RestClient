using VW.Api.RestClient;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Providers;

namespace VW.PCI.Api.Client.Sample.Infrastructure
{
    /// <summary>
    /// Gọi PCIClientFactory.Initialize() 1 lần duy nhất khi app start.
    /// Sau đó dùng PCIServiceClient.Instance ở bất kỳ đâu trong app.
    /// </summary>
    public static class PCIClientFactory
    {
        public static void Initialize()
        {
            var credentials = new AuthTokenRequest
            {
                ApplicationId   = "your-application-id",
                ApplicationName = "VisionWeb",
                ApplicationCode = "your-application-code"
            };

            var logger         = new SampleLogger();
            var loggingService = new SampleLoggingService(logger);
            var tokenProvider  = new SamplePCITokenProvider(credentials, logger);

            ApiSettingsManager.Setup(settingFilesDirectory: @"App_Data\ApiSettings");

            PCIServiceClient.Setup(logger, loggingService, tokenProvider);
        }
    }
}
