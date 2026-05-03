using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS.AO.Api.RestClient.Models
{
   public class MockService
    {
        public string Url { get; set; }
        public string Environment { get; set; }
        public string Module { get; set; }
        public string Source { get; set; }
        public string Version { get; set; }
        public bool? Enabled { get; set; }
        public string Name { get; set; }


        public void AddHeaderRequest(IRestRequest request)
        {
            request.AddHeader("mock_environment", this.Environment);
            request.AddHeader("mock_module", this.Module);
            request.AddHeader("mock_source", this.Source);
            request.AddHeader("mock_version", this.Version);
            request.AddHeader("mock_operation", this.Name);
        }
    }
}
