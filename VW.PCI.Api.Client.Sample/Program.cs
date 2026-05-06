using System;
using VW.Api.RestClient;
using VW.PCI.Api.Client;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Examples;
using VW.PCI.Api.Client.Sample.Infrastructure;

namespace VW.PCI.Api.Client.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== VW.PCI.Api.Client — Sample Usage ===\n");

            // ------------------------------------------------------------------
            // Setup 1 lần khi app start
            // Trong ASP.NET: đặt ở Application_Start (Global.asax) hoặc Startup.cs
            // ------------------------------------------------------------------
            ApiSettingsManager.Setup(settingFilesDirectory: @"App_Data\ApiSettings");

            PCIServiceClient.Configure(
                logger:         SampleLogger.Instance,
                loggingService: SampleLoggingService.Instance,
                credentials:    new AuthTokenRequest
                {
                    ApplicationId   = "your-application-id",
                    ApplicationName = "VisionWeb",
                    ApplicationCode = "your-application-code"
                }
            );

            var examples = new UserExamples();

            try
            {
                examples.GetUsers();
                examples.CreateUser();
                examples.UpdateUser();
                examples.GetMasterMerchant();
                examples.GetHierarchyID();
                examples.GetAllHierarchyForAO();
                examples.UpdSecRoleByUserID();
                examples.UpdateOptInOut();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[FATAL] {ex.Message}");
            }

            Console.WriteLine("\n=== Done. Press any key to exit. ===");
            Console.ReadKey();
        }
    }
}
