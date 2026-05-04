using System;
using System.Collections.Generic;

namespace VW.PCI.Api.Client.Models.Responses
{
    public class GetHierarchyIDResponse
    {
        public List<HierarchyIDItem> Data { get; set; }
    }

    public class HierarchyIDItem
    {
        public string HierarchyID { get; set; }
        public string SystemId { get; set; }
        public string HierarchyName { get; set; }
        public string HierarchyParent { get; set; }
        public string HierarchyDescription { get; set; }
        public string ActiveStatus { get; set; }
        public string ClientId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string HierarchyCode { get; set; }
    }
}
