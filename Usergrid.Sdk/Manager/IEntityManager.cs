using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{
    public interface IEntityManager
    {
        UsergridEntity<T> CreateEntity<T>(string collection, T entity = null) where T : class;
        void DeleteEntity(string collection, string identifer /*name or uuid*/);
        void UpdateEntity<T>(string collection, string identifer /*name or uuid*/, T entity);
        UsergridEntity<T> GetEntity<T>(string collectionName, string identifer /*name or uuid*/);
		UsergridCollection<UsergridEntity<T>> GetEntities<T> (string collectionName, int limit = 10, string query = null);
		UsergridCollection<UsergridEntity<T>> GetNextEntities<T>(string collectionName, string query = null);
		UsergridCollection<UsergridEntity<T>> GetPreviousEntities<T>(string collectionName, string query = null);
    }
}