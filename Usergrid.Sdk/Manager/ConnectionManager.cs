using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Manager
{
    internal class ConnectionManager : ManagerBase, IConnectionManager
    {
        internal ConnectionManager(IUsergridRequest request) : base(request)
        {
        }

        public async Task CreateConnection(Connection connection) 
        {
            // e.g. /user/fred/following/user/barney
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format(
                "/{0}/{1}/{2}/{3}/{4}",
                connection.ConnectorCollectionName,
                connection.ConnectorIdentifier,
                connection.ConnectionName,
                connection.ConnecteeCollectionName,
                connection.ConnecteeIdentifier), HttpMethod.Post);

            ValidateResponse(response);
        }

        public async Task<IList<UsergridEntity>> GetConnections(Connection connection) 
        {
            // e.g. /user/fred/following
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/{0}/{1}/{2}",
                                                                              connection.ConnectorCollectionName,
                                                                              connection.ConnectorIdentifier,
                                                                              connection.ConnectionName), HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(List<UsergridEntity>);
            }

            ValidateResponse(response);

            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity>>(response.Content);

            return entity.Entities;
        }

        public async Task<IList<TConnectee>> GetConnections<TConnectee>(Connection connection) 
        {
            // e.g. /user/fred/following/user
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/{0}/{1}/{2}/{3}",
                                                                              connection.ConnectorCollectionName,
                                                                              connection.ConnectorIdentifier, 
                                                                              connection.ConnectionName,
                                                                              connection.ConnecteeCollectionName), HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(List<TConnectee>);
            }

            ValidateResponse(response);

            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<TConnectee>>(response.Content);

            return entity.Entities;
        }

        public async Task DeleteConnection(Connection connection)
        {
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format(
                "/{0}/{1}/{2}/{3}/{4}",
                connection.ConnectorCollectionName,
                connection.ConnectorIdentifier,
                connection.ConnectionName,
                connection.ConnecteeCollectionName,
                connection.ConnecteeIdentifier), HttpMethod.Delete);

            ValidateResponse(response);
        }
    }
}