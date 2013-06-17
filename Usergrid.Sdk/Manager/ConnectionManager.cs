using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Manager
{
    internal class ConnectionManager : ManagerBase, IConnectionManager
    {
        internal ConnectionManager(IUsergridRequest request) : base(request)
        {
        }

        #region IConnectionManager Members

        public void CreateConnection<TConnector, TConnectee>(TConnector connector, TConnectee connectee, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity
        {
            // e.g. /user/fred/following/user/barney
            IRestResponse response = Request.ExecuteJsonRequest(string.Format(
                "/{0}/{1}/{2}/{3}/{4}",
                connector.Type,
                connector.Name,
                connection,
                connectee.Type,
                connectee.Name), Method.POST);

            ValidateResponse(response);
        }

        public IList<UsergridEntity> GetConnections<TConnector>(TConnector connector, string connection) where TConnector : UsergridEntity
        {
            // e.g. /user/fred/following
            IRestResponse response = Request.ExecuteJsonRequest(string.Format("/{0}/{1}/{2}",
                                                                              connector.Type,
                                                                              connector.Name,
                                                                              connection), Method.GET);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(List<UsergridEntity>);
            }

            ValidateResponse(response);

            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity>>(response.Content);

            return entity.Entities;
        }

        public IList<UsergridEntity<TConnectee>> GetConnections<TConnector, TConnectee>(TConnector connector, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity
        {
            // e.g. /user/fred/following/user
            IRestResponse response = Request.ExecuteJsonRequest(string.Format("/{0}/{1}/{2}/{3}",
                                                                              connector.Type,
                                                                              connector.Name,
                                                                              connection,
                                                                              typeof (TConnectee).Name), Method.GET);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default(List<UsergridEntity<TConnectee>>);
            }

            ValidateResponse(response);

            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity<TConnectee>>>(response.Content);

            return entity.Entities;
        }

        public void DeleteConnection<TConnector, TConnectee>(TConnector connector, TConnectee connectee, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity
        {
            IRestResponse response = Request.ExecuteJsonRequest(string.Format(
                "/{0}/{1}/{2}/{3}/{4}",
                connector.Type,
                connector.Name,
                connection,
                connectee.Type,
                connectee.Name), Method.DELETE);

            ValidateResponse(response);
        }

        #endregion
    }
}