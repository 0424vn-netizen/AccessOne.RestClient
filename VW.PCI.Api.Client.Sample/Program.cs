using System;
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
            // BƯỚC 1: Setup 1 lần khi app start
            // Trong ASP.NET: gọi ở Application_Start (Global.asax) hoặc Startup.cs
            // ------------------------------------------------------------------
            PCIClientFactory.Initialize();

            // ------------------------------------------------------------------
            // BƯỚC 2: Dùng PCIServiceClient.Instance ở bất kỳ đâu — không cần new, không cần inject
            // ------------------------------------------------------------------
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
                Console.WriteLine($"\n[FATAL] Unhandled exception: {ex.Message}");
            }

            Console.WriteLine("\n=== Done. Press any key to exit. ===");
            Console.ReadKey();
        }
    }
}
