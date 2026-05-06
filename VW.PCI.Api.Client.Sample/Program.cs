using System;
using VW.Api.RestClient;
using VW.PCI.Api.Client;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Sample.Infrastructure;

namespace VW.PCI.Api.Client.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== VW.PCI.Api.Client — Sample Usage ===\n");

            // Setup 1 lần khi app start
            ApiSettingsManager.Setup(settingFilesDirectory: @"App_Data\ApiSettings");
            PCIServiceClient.Configure(
                logger:         SampleLogger.Instance,
                loggingService: SampleLoggingService.Instance
            );

            // Credentials lấy từ user đang login
            var client = PCIServiceClient.ForUser(new AuthTokenRequest
            {
                ApplicationId   = "user-application-id",
                ApplicationName = "VisionWeb",
                ApplicationCode = "user-application-code"
            });

            try
            {
                // GetUsers
                var users = client.GetUsers(new GetUsersRequest { ASClient = "64", UserName = "john.doe" });
                if (users.IsSuccess && users.Data?.Data != null)
                    foreach (var u in users.Data.Data)
                        Console.WriteLine($"  [{u.UserID}] {u.FullName} — {u.Email}");
                else
                    Console.WriteLine($"  GetUsers FAILED: {users.ErrorMessage}");

                // CreateUser
                var created = client.CreateUser(new CreateUserRequest
                {
                    UserName            = "john.doe",
                    AsClient            = "64",
                    Email               = "john.doe@example.com",
                    FirstName           = "John",
                    LastName            = "Doe",
                    FullName            = "John Doe",
                    Password            = "P@ssword123",
                    PasswordType        = "1",
                    LoginQuestionIndex  = 1,
                    LoginQuestionAnswer = "Fluffy",
                    ActiveStatus        = "1",
                    HierarchyIds        = "H001,H002",
                    CreatedBy           = "admin",
                    UserSecRole         = "MERCHANT",
                    PciAccess           = true,
                    EntityID            = "E001",
                    EntityTypeID        = "ET001"
                });
                Console.WriteLine(created.IsSuccess
                    ? $"  CreateUser OK — TrackingId: {created.TrackingId}"
                    : $"  CreateUser FAILED: {created.ErrorMessage}");

                // UpdateUser
                var updated = client.UpdateUser(new UpdateUserRequest
                {
                    RecId               = "550e8400-e29b-41d4-a716-446655440000",
                    UserName            = "john.doe",
                    AsClient            = "64",
                    Email               = "john.new@example.com",
                    FirstName           = "John",
                    LastName            = "Doe",
                    FullName            = "John Doe",
                    PasswordType        = "1",
                    LoginQuestionIndex  = 2,
                    LoginQuestionAnswer = "Buddy",
                    ActiveStatus        = "1",
                    HierarchyIds        = "H001,H002,H003",
                    PciAccess           = true
                });
                Console.WriteLine(updated.IsSuccess
                    ? $"  UpdateUser OK — TrackingId: {updated.TrackingId}"
                    : $"  UpdateUser FAILED: {updated.ErrorMessage}");

                // GetMasterMerchant
                var merchant = client.GetMasterMerchant(new GetMasterMerchantRequest { ASClient = "64", PrimaryUserId = "john.doe" });
                if (merchant.IsSuccess)
                    Console.WriteLine($"  MerchantNumber: {merchant.Data.MerchantNumber} — Status: {merchant.Data.Status}");
                else
                    Console.WriteLine($"  GetMasterMerchant FAILED: {merchant.ErrorMessage}");

                // GetHierarchyID
                var hierarchy = client.GetHierarchyID(new GetHierarchyIDRequest
                {
                    ASClient        = "64",
                    HierarchyCode   = "H001",
                    HierarchyParent = "",
                    ActiveStatus    = "1",
                    SystemId        = ""
                });
                if (hierarchy.IsSuccess && hierarchy.Data?.Data != null)
                    foreach (var h in hierarchy.Data.Data)
                        Console.WriteLine($"  {h.HierarchyID} — {h.HierarchyName} ({h.HierarchyCode})");
                else
                    Console.WriteLine($"  GetHierarchyID FAILED: {hierarchy.ErrorMessage}");

                // GetAllHierarchyForAO
                var allHierarchy = client.GetAllHierarchyForAO(new GetAllHierarchyForAORequest { ASClient = "64", UserSecRole = "MERCHANT" });
                if (allHierarchy.IsSuccess && allHierarchy.Data?.Data != null)
                    Console.WriteLine($"  GetAllHierarchyForAO — Total: {allHierarchy.Data.Data.Count}");
                else
                    Console.WriteLine($"  GetAllHierarchyForAO FAILED: {allHierarchy.ErrorMessage}");

                // UpdSecRoleByUserID
                var secRole = client.UpdSecRoleByUserID(new UpdSecRoleByUserIDRequest
                {
                    ASClient          = "64",
                    UserID            = "john.doe",
                    RoleID            = "ROLE_ADMIN",
                    HierarchyEntityID = "H001"
                });
                Console.WriteLine(secRole.IsSuccess
                    ? $"  UpdSecRoleByUserID OK — TrackingId: {secRole.TrackingId}"
                    : $"  UpdSecRoleByUserID FAILED: {secRole.ErrorMessage}");

                // UpdateOptInOut
                var optInOut = client.UpdateOptInOut(new UpdateOptInOutRequest { ASClient = "64", UserID = "john.doe", IsActive = false });
                Console.WriteLine(optInOut.IsSuccess
                    ? $"  UpdateOptInOut OK — TrackingId: {optInOut.TrackingId}"
                    : $"  UpdateOptInOut FAILED: {optInOut.ErrorMessage}");
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
