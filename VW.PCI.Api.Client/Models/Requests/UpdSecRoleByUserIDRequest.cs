namespace VW.PCI.Api.Client.Models.Requests
{
    public class UpdSecRoleByUserIDRequest
    {
        public string ASClient { get; set; }
        public string UserID { get; set; }
        public string RoleID { get; set; }
        public string HierarchyEntityID { get; set; }
    }
}
