using System;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;

namespace VW.PCI.Api.Client.Sample.Examples
{
    public class UserExamples
    {
        // Dùng thẳng PCIClient.Instance — giống StatementClient.Instance.Method(...)
        private static IPCIServiceClient Client => PCIClient.Instance;

        public void CreateUser()
        {
            Console.WriteLine("\n=== CreateUser ===");
            var result = Client.CreateUser(new CreateUserRequest
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
            PrintResult(result);
        }

        public void UpdateUser()
        {
            Console.WriteLine("\n=== UpdateUser ===");
            var result = Client.UpdateUser(new UpdateUserRequest
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
            PrintResult(result);
        }

        public void GetMasterMerchant()
        {
            Console.WriteLine("\n=== GetMasterMerchant ===");
            var result = Client.GetMasterMerchant(new GetMasterMerchantRequest
            {
                ASClient      = "64",
                PrimaryUserId = "john.doe"
            });
            if (result.IsSuccess)
            {
                Console.WriteLine($"  MerchantNumber : {result.Data.MerchantNumber}");
                Console.WriteLine($"  Status         : {result.Data.Status}");
            }
            else PrintError(result);
        }

        public void GetHierarchyID()
        {
            Console.WriteLine("\n=== GetHierarchyID ===");
            var result = Client.GetHierarchyID(new GetHierarchyIDRequest
            {
                ASClient        = "64",
                HierarchyCode   = "H001",
                HierarchyParent = "",
                ActiveStatus    = "1",
                SystemId        = ""
            });
            if (result.IsSuccess && result.Data?.Data != null)
                foreach (var item in result.Data.Data)
                    Console.WriteLine($"  {item.HierarchyID} — {item.HierarchyName} ({item.HierarchyCode})");
            else PrintError(result);
        }

        public void UpdSecRoleByUserID()
        {
            Console.WriteLine("\n=== UpdSecRoleByUserID ===");
            var result = Client.UpdSecRoleByUserID(new UpdSecRoleByUserIDRequest
            {
                ASClient          = "64",
                UserID            = "john.doe",
                RoleID            = "ROLE_ADMIN",
                HierarchyEntityID = "H001"
            });
            PrintResult(result);
        }

        public void UpdateOptInOut()
        {
            Console.WriteLine("\n=== UpdateOptInOut ===");
            var result = Client.UpdateOptInOut(new UpdateOptInOutRequest
            {
                ASClient = "64",
                UserID   = "john.doe",
                IsActive = false
            });
            PrintResult(result);
        }

        public void GetAllHierarchyForAO()
        {
            Console.WriteLine("\n=== GetAllHierarchyForAO ===");
            var result = Client.GetAllHierarchyForAO(new GetAllHierarchyForAORequest
            {
                ASClient    = "64",
                UserSecRole = "MERCHANT"
            });
            if (result.IsSuccess && result.Data?.Data != null)
            {
                Console.WriteLine($"  Total: {result.Data.Data.Count}");
                foreach (var item in result.Data.Data)
                    Console.WriteLine($"  [{item.HierarchyLevel}] {item.HierarchyID} — {item.HierarchyName}");
            }
            else PrintError(result);
        }

        public void GetUsers()
        {
            Console.WriteLine("\n=== GetUsers ===");
            var result = Client.GetUsers(new GetUsersRequest
            {
                ASClient = "64",
                UserName = "john.doe"
            });
            if (result.IsSuccess && result.Data?.Data != null)
                foreach (var user in result.Data.Data)
                {
                    Console.WriteLine($"  RecId        : {user.RecId}");
                    Console.WriteLine($"  UserID       : {user.UserID}");
                    Console.WriteLine($"  FullName     : {user.FullName}");
                    Console.WriteLine($"  Email        : {user.Email}");
                    Console.WriteLine($"  ActiveStatus : {user.ActiveStatus}");
                    Console.WriteLine($"  UserSecRole  : {user.UserSecRole}");
                    Console.WriteLine($"  HierarchyId  : {user.HierarchyId}");
                }
            else PrintError(result);
        }

        private static void PrintResult<T>(PCIApiResponse<T> result)
        {
            if (result.IsSuccess)
                Console.WriteLine($"  SUCCESS — TrackingId: {result.TrackingId}");
            else
                PrintError(result);
        }

        private static void PrintError<T>(PCIApiResponse<T> result) =>
            Console.WriteLine($"  FAILED — Status: {result.StatusCode} | Error: {result.ErrorMessage}");
    }
}
