using System.Net;

namespace VW.PCI.Api.Client.Models.Common
{
    public class PCIApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string TrackingId { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }
}
