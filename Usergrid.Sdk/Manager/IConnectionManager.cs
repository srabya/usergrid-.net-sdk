using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{

    internal interface IConnectionManager
    {
        void CreateConnection<TConnector,TConnectee>(TConnector connector, TConnectee connectee, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity;
		IList<UsergridEntity> GetConnections<TConnector>(TConnector connector, string connection) where TConnector : Usergrid.Sdk.Model.UsergridEntity;
        IList<UsergridEntity<TConnectee>> GetConnections<TConnector, TConnectee>(TConnector connector, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity;
        void DeleteConnection<TConnector,TConnectee>(TConnector connector, TConnectee connectee, string connection) where TConnector : UsergridEntity where TConnectee : UsergridEntity;
    }
    
}