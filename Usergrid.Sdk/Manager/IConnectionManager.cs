using System.Collections.Generic;
using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager {
    internal interface IConnectionManager {
        Task CreateConnection(Connection connection);
        Task<IList<UsergridEntity>> GetConnections(Connection connection);
        Task<IList<TConnectee>> GetConnections<TConnectee>(Connection connection);
        Task DeleteConnection(Connection connection);
    }
}