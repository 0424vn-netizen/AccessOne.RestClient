using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AS.AO.Api.RestClient.Models
{
    public class MockResponse<TResult>
    {
        public HttpStatusCode StatusCode { get; set; }
        public TResult ResponseData { get; set; }

        public void DeserializeObject(string response)
        {
            var result = JsonConvert.DeserializeObject<MockResponse<TResult>>(response);
            if (result.ResponseData == null)
            {
                result.ResponseData = JsonConvert.DeserializeObject<TResult>(response);
            }
            this.StatusCode = result.StatusCode;
            this.ResponseData = result.ResponseData;
        }
    }
}
