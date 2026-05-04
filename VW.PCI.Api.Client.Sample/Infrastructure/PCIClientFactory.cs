using VW.Api.RestClient;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Providers;

namespace VW.PCI.Api.Client.Sample.Infrastructure
{
    /// <summary>
    /// Factory khởi tạo PCIServiceClient với đầy đủ dependencies.
    /// Trong project thực tế, đăng ký các class này vào DI container (Autofac, Unity, .NET DI...)
    /// thay vì dùng factory thủ công.
    /// </summary>
    public static class PCIClientFactory
    {
        /// <summary>
        /// Tạo PCIServiceClient sẵn sàng dùng.
        /// Gọi 1 lần khi app khởi động, giữ instance dùng lại (singleton).
        /// </summary>
        public static IPCIServiceClient Create(string baseUrl = null)
        {
            // 1. Cấu hình credentials để lấy token
            var credentials = new AuthTokenRequest
            {
                ApplicationId   = "your-application-id",
                ApplicationName = "VisionWeb",
                ApplicationCode = "your-application-code"
            };

            // 2. Khởi tạo logger và logging service
            var logger         = new SampleLogger();
            var loggingService = new SampleLoggingService(logger);

            // 3. Chọn token provider:
            //    - SamplePCITokenProvider : lưu DB (ví dụ trong project này)
            //    - PCIInMemoryTokenProvider: lưu static memory (dùng cho dev/test nhanh)
            var tokenProvider = new SamplePCITokenProvider(credentials, logger);

            // 4. Setup đường dẫn file config — gọi 1 lần duy nhất khi app start
            ApiSettingsManager.Setup(
                settingFilesDirectory: @"App_Data\ApiSettings"
            );

            return new PCIServiceClient(logger, loggingService, tokenProvider);
        }
    }
}
