namespace VW.PCI.Api.Client.Models.Requests
{
    public class UpdateOptInOutRequest
    {
        public string ASClient { get; set; }
        public string UserID { get; set; }
        public bool IsActive { get; set; }
    }
}
