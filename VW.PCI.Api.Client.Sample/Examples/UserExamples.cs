using System;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client.Sample.Examples
{
    /// <summary>
    /// Ví dụ sử dụng chi tiết từng API của IPCIServiceClient.
    /// </summary>
    public class UserExamples
    {
        private readonly IPCIServiceClient _client;

        public UserExamples(IPCIServiceClient client)
        {
            _client = client;
        }

        // =====================================================================
        // 1. CreateUser — Tạo user mới
        // =====================================================================
        public void CreateUser()
        {
            Console.WriteLine("\n=== CreateUser ===");

            var result = _client.CreateUser(new CreateUserRequest
            {
                UserName           = "john.doe",
                AsClient           = "64",
                Email              = "john.doe@example.com",
                FirstName          = "John",
                LastName           = "Doe",
                FullName           = "John Doe",
                Password           = "P@ssword123",
                PasswordType       = "1",
                LoginQuestionIndex = 1,
                LoginQuestionAnswer= "Fluffy",
                ActiveStatus       = "1",
                HierarchyIds       = "H001,H002",
                CreatedBy          = "admin",
                UserSecRole        = "MERCHANT",
                PciAccess          = true,
                EntityID           = "E001",
                EntityTypeID       = "ET001"
            });

            PrintResult(result);
            // result.Data.Message — trạng thái tạo user
        }

        // =====================================================================
        // 2. UpdateUser — Cập nhật thông tin user
        // =====================================================================
        public void UpdateUser()
        {
            Console.WriteLine("\n=== UpdateUser ===");

            var result = _client.UpdateUser(new UpdateUserRequest
            {
                RecId              = "550e8400-e29b-41d4-a716-446655440000", // GUID từ GetUsers
                UserName           = "john.doe",
                AsClient           = "64",
                Email              = "john.doe.new@example.com",
                FirstName          = "John",
                LastName           = "Doe",
                FullName           = "John Doe",
                PasswordType       = "1",
                LoginQuestionIndex = 2,
                LoginQuestionAnswer= "Buddy",
                ActiveStatus       = "1",
                HierarchyIds       = "H001,H002,H003",
                PciAccess          = true
                // SystemID: bỏ trống → giữ nguyên giá trị hiện tại
            });

            PrintResult(result);
        }

        // =====================================================================
        // 3. GetMasterMerchant — Lấy merchant number theo user
        // =====================================================================
        public void GetMasterMerchant()
        {
            Console.WriteLine("\n=== GetMasterMerchant ===");

            var result = _client.GetMasterMerchant(new GetMasterMerchantRequest
            {
                ASClient      = "64",
                PrimaryUserId = "john.doe"
            });

            if (result.IsSuccess)
            {
                Console.WriteLine($"MerchantNumber : {result.Data.MerchantNumber}");
                Console.WriteLine($"Status         : {result.Data.Status}");
            }
            else
            {
                PrintError(result);
            }
        }

        // =====================================================================
        // 4. GetHierarchyID — Tìm hierarchy theo tiêu chí
        // =====================================================================
        public void GetHierarchyID()
        {
            Console.WriteLine("\n=== GetHierarchyID ===");

            var result = _client.GetHierarchyID(new GetHierarchyIDRequest
            {
                ASClient        = "64",
                HierarchyCode   = "H001",
                HierarchyParent = "",
                ActiveStatus    = "1",
                SystemId        = ""
            });

            if (result.IsSuccess && result.Data?.Data != null)
            {
                foreach (var item in result.Data.Data)
                {
                    Console.WriteLine($"  HierarchyID={item.HierarchyID} | Name={item.HierarchyName} | Code={item.HierarchyCode}");
                }
            }
            else
            {
                PrintError(result);
            }
        }

        // =====================================================================
        // 5. UpdSecRoleByUserID — Cập nhật security role
        // =====================================================================
        public void UpdSecRoleByUserID()
        {
            Console.WriteLine("\n=== UpdSecRoleByUserID ===");

            var result = _client.UpdSecRoleByUserID(new UpdSecRoleByUserIDRequest
            {
                ASClient          = "64",
                UserID            = "john.doe",
                RoleID            = "ROLE_ADMIN",
                HierarchyEntityID = "H001"
            });

            PrintResult(result);
        }

        // =====================================================================
        // 6. UpdateOptInOut — Bật/tắt trạng thái active của user
        // =====================================================================
        public void UpdateOptInOut()
        {
            Console.WriteLine("\n=== UpdateOptInOut ===");

            // Tắt user
            var result = _client.UpdateOptInOut(new UpdateOptInOutRequest
            {
                ASClient = "64",
                UserID   = "john.doe",
                IsActive = false
            });

            PrintResult(result);
        }

        // =====================================================================
        // 7. GetAllHierarchyForAO — Lấy toàn bộ hierarchy
        // =====================================================================
        public void GetAllHierarchyForAO()
        {
            Console.WriteLine("\n=== GetAllHierarchyForAO ===");

            var result = _client.GetAllHierarchyForAO(new GetAllHierarchyForAORequest
            {
                ASClient    = "64",   // default value
                UserSecRole = "MERCHANT"
            });

            if (result.IsSuccess && result.Data?.Data != null)
            {
                Console.WriteLine($"Total hierarchies: {result.Data.Data.Count}");
                foreach (var item in result.Data.Data)
                {
                    Console.WriteLine($"  [{item.HierarchyLevel}] {item.HierarchyID} — {item.HierarchyName} (Parent: {item.HierarchyParent})");
                }
            }
            else
            {
                PrintError(result);
            }
        }

        // =====================================================================
        // 8. GetUsers — Tìm user theo username
        // =====================================================================
        public void GetUsers()
        {
            Console.WriteLine("\n=== GetUsers ===");

            var result = _client.GetUsers(new GetUsersRequest
            {
                ASClient = "64",
                UserName = "john.doe"
            });

            if (result.IsSuccess && result.Data?.Data != null)
            {
                foreach (var user in result.Data.Data)
                {
                    Console.WriteLine($"  RecId       : {user.RecId}");
                    Console.WriteLine($"  UserID      : {user.UserID}");
                    Console.WriteLine($"  FullName    : {user.FullName}");
                    Console.WriteLine($"  Email       : {user.Email}");
                    Console.WriteLine($"  ActiveStatus: {user.ActiveStatus}");
                    Console.WriteLine($"  UserSecRole : {user.UserSecRole}");
                    Console.WriteLine($"  HierarchyId : {user.HierarchyId}");
                    Console.WriteLine($"  LastLogin   : {user.LastLoginDTS}");
                }
            }
            else
            {
                PrintError(result);
            }
        }

        // =====================================================================
        // Helper — in kết quả chung
        // =====================================================================
        private static void PrintResult<T>(PCIApiResponse<T> result)
        {
            if (result.IsSuccess)
                Console.WriteLine($"  SUCCESS — TrackingId: {result.TrackingId}");
            else
                PrintError(result);
        }

        private static void PrintError<T>(PCIApiResponse<T> result)
        {
            Console.WriteLine($"  FAILED — Status: {result.StatusCode} | TrackingId: {result.TrackingId}");
            Console.WriteLine($"  Error : {result.ErrorMessage}");
        }
    }
}
