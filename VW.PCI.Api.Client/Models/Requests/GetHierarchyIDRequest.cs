namespace VW.PCI.Api.Client.Models.Requests
{
    public class GetHierarchyIDRequest
    {
        public string SystemId { get; set; }
        public string HierarchyParent { get; set; }
        public string ActiveStatus { get; set; }
        public string ASClient { get; set; }
        public string HierarchyCode { get; set; }
    }
}
