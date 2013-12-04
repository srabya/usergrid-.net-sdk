using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{
    internal interface IEntityManager
    {
        Task<T> CreateEntity<T>(string collection, T entity);
        Task DeleteEntity(string collection, string identifer /*name or uuid*/);
        Task UpdateEntity<T>(string collection, string identifer /*name or uuid*/, T entity);
        Task<T> GetEntity<T>(string collectionName, string identifer /*name or uuid*/);
		Task<UsergridCollection<T>> GetEntities<T> (string collectionName, int limit = 10, string query = null);
		Task<UsergridCollection<T>> GetNextEntities<T>(string collectionName, string query = null);
		Task<UsergridCollection<T>> GetPreviousEntities<T>(string collectionName, string query = null);
    }
}