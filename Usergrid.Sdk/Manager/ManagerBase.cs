using System.Net;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{
	internal abstract class ManagerBase
    {
        protected readonly IUsergridRequest Request;

        internal ManagerBase(IUsergridRequest request)
        {
            Request = request;
        }

        protected void ValidateResponse(IRestResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var userGridError = JsonConvert.DeserializeObject<UsergridError>(response.Content);
                throw new UsergridException(userGridError);
            }
        }
    }
}