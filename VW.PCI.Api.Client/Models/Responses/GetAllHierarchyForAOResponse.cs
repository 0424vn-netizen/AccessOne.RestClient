using System.Collections.Generic;

namespace VW.PCI.Api.Client.Models.Responses
{
    public class GetAllHierarchyForAOResponse
    {
        public List<HierarchyForAOItem> Data { get; set; }
    }

    public class HierarchyForAOItem
    {
        public string HierarchyID { get; set; }
        public string SystemId { get; set; }
        public string HierarchyName { get; set; }
        public string HierarchyParent { get; set; }
        public string HierarchyDescription { get; set; }
        public string ActiveStatus { get; set; }
        public string HierarchyCode { get; set; }
        public string HierarchyLevel { get; set; }
    }
}
