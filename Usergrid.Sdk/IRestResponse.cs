using System.Net;
using System.Net.Http;

namespace Usergrid.Sdk {
    public interface IRestResponse
    {
        string Content { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
    public class RestResponse : IRestResponse
    {
        public RestResponse(string data, HttpResponseMessage response)
        {
            Content = data;
            StatusCode = response.StatusCode;
        }

        public string Content { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

}