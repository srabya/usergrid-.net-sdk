using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Usergrid.Sdk.Manager;

namespace Usergrid.Sdk
{
    public interface IUsergridRequest
    {
        Task<IRestResponse> ExecuteJsonRequest(string resource, HttpMethod method, object body = null);
        string AccessToken { get; set; }
    }
}