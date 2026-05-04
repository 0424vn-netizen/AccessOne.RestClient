using System;
using VW.PCI.Api.Client.Sample.Examples;
using VW.PCI.Api.Client.Sample.Infrastructure;

namespace VW.PCI.Api.Client.Sample
{
    /// <summary>
    /// Entry point — minh họa toàn bộ flow sử dụng VW.PCI.Api.Client.
    ///
    /// Cấu trúc project thực tế (ASP.NET, WinForms, Console...):
    ///   1. Đăng ký IPCIServiceClient vào DI container (singleton)
    ///   2. Inject IPCIServiceClient vào Service/Controller cần dùng
    ///   3. Gọi method tương ứng
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== VW.PCI.Api.Client — Sample Usage ===\n");

            // ------------------------------------------------------------------
            // BƯỚC 1: Khởi tạo client (làm 1 lần duy nhất, giữ singleton)
            // ------------------------------------------------------------------
            // Trong ASP.NET: đăng ký vào DI ở Startup.cs / Global.asax
            //   container.RegisterInstance<IPCIServiceClient>(PCIClientFactory.Create());
            //
            // Trong Console / WinForms: tạo thủ công như dưới
            var pciClient = PCIClientFactory.Create();

            // ------------------------------------------------------------------
            // BƯỚC 2: Chạy các ví dụ
            // ------------------------------------------------------------------
            var examples = new UserExamples(pciClient);

            try
            {
                // Token được fetch tự động ở lần gọi đầu tiên,
                // các lần sau dùng lại từ DB/cache cho đến khi hết hạn.

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
