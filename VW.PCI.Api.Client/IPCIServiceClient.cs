using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;
using VW.PCI.Api.Client.Models.Responses;

namespace VW.PCI.Api.Client
{
    public interface IPCIServiceClient
    {
        PCIApiResponse<CreateUserResponse> CreateUser(CreateUserRequest request);
        PCIApiResponse<UpdateUserResponse> UpdateUser(UpdateUserRequest request);
        PCIApiResponse<GetMasterMerchantResponse> GetMasterMerchant(GetMasterMerchantRequest request);
        PCIApiResponse<GetHierarchyIDResponse> GetHierarchyID(GetHierarchyIDRequest request);
        PCIApiResponse<UpdSecRoleByUserIDResponse> UpdSecRoleByUserID(UpdSecRoleByUserIDRequest request);
        PCIApiResponse<UpdateOptInOutResponse> UpdateOptInOut(UpdateOptInOutRequest request);
        PCIApiResponse<GetAllHierarchyForAOResponse> GetAllHierarchyForAO(GetAllHierarchyForAORequest request);
        PCIApiResponse<GetUsersResponse> GetUsers(GetUsersRequest request);
    }
}
