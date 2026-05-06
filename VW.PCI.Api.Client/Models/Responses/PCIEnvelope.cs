namespace VW.PCI.Api.Client.Models.Responses
{
    internal class PCIEnvelope<T>
    {
        public T Data { get; set; }
        public object ErrorMessages { get; set; }
    }
}
