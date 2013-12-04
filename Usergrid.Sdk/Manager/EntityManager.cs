using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Manager {
    internal class EntityManager : ManagerBase, IEntityManager {
        private readonly IDictionary<Type, Stack<string>> _cursorStates = new Dictionary<Type, Stack<string>>();
        private readonly IDictionary<Type, int> _pageSizes = new Dictionary<Type, int>();

        internal EntityManager(IUsergridRequest request) : base(request) {}

        public async Task<T> CreateEntity<T>(string collection, T entity) {
            IRestResponse response = await Request.ExecuteJsonRequest("/" + collection, HttpMethod.Post, entity);
            ValidateResponse(response);
            var returnedEntity = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);

            return returnedEntity.Entities.FirstOrDefault();
        }

        public async Task DeleteEntity(string collection, string identifer) {
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/{0}/{1}", collection, identifer), HttpMethod.Delete);
            ValidateResponse(response);
        }

        public async Task UpdateEntity<T>(string collection, string identifer, T entity) {
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/{0}/{1}", collection, identifer), HttpMethod.Put, entity);
            ValidateResponse(response);
        }

        public async Task<T> GetEntity<T>(string collectionName, string identifer) {
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/{0}/{1}", collectionName, identifer), HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound || (response.StatusCode == HttpStatusCode.Unauthorized && response.Content.Contains("not_found")))
                return default(T);
            ValidateResponse(response);

            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);

            return entity.Entities.FirstOrDefault();
        }

        public async Task<UsergridCollection<T>> GetEntities<T>(string collectionName, int limit = 10, string query = null) {
            _pageSizes.Remove(typeof (T));
            _pageSizes.Add(typeof (T), limit);

            string url = string.Format("/{0}?limit={1}", collectionName, limit);
            if (query != null)
                url += "&query=" + query;

            IRestResponse response = await Request.ExecuteJsonRequest(url, HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new UsergridCollection<T>();

            ValidateResponse(response);

            var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            var collection = new UsergridCollection<T>(getResponse.Entities);

            _cursorStates.Remove(typeof (T));

            if (getResponse.Cursor != null) {
                collection.HasNext = true;
                collection.HasPrevious = false;
                _cursorStates.Add(typeof (T), new Stack<string>(new[] {null, null, getResponse.Cursor}));
            }

            return collection;
        }

        public async Task<UsergridCollection<T>> GetNextEntities<T>(string collectionName, string query = null) {
            if (!_cursorStates.ContainsKey(typeof (T))) {
                return new UsergridCollection<T>();
            }

            Stack<string> stack = _cursorStates[typeof (T)];
            string cursor = stack.Peek();

            if (cursor == null) {
                return new UsergridCollection<T> {HasNext = false, HasPrevious = true};
            }

            int limit = _pageSizes[typeof (T)];

            string url = string.Format("/{0}?cursor={1}&limit={2}", collectionName, cursor, limit);
            if (query != null)
                url += "&query=" + query;

            IRestResponse response = await Request.ExecuteJsonRequest(url, HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new UsergridCollection<T>();

            ValidateResponse(response);

            var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            var collection = new UsergridCollection<T>(getResponse.Entities);

            if (getResponse.Cursor != null) {
                collection.HasNext = true;
                stack.Push(getResponse.Cursor);
            }
            else {
                stack.Push(null);
            }

            collection.HasPrevious = true;

            return collection;
        }

        public async Task<UsergridCollection<T>> GetPreviousEntities<T>(string collectionName, string query = null) {
            if (!_cursorStates.ContainsKey(typeof (T))) {
                var error = new UsergridError
                    {
                        Error = "cursor_not_initialized",
                        Description = "Call GetEntities method to initialize the cursor"
                    };
                throw new UsergridException(error);
            }

            Stack<string> stack = _cursorStates[typeof (T)];
            stack.Pop();
            stack.Pop();
            string cursor = stack.Peek();

            int limit = _pageSizes[typeof (T)];

            if (cursor == null) {
                return await GetEntities<T>(collectionName, limit);
            }

            string url = string.Format("/{0}?cursor={1}&limit={2}", collectionName, cursor, limit);
            if (query != null)
                url += "&query=" + query;

            IRestResponse response = await Request.ExecuteJsonRequest(url, HttpMethod.Get);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return new UsergridCollection<T>();

            ValidateResponse(response);

            var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            var collection = new UsergridCollection<T>(getResponse.Entities)
                {
                    HasNext = true, 
                    HasPrevious = true
                };

            stack.Push(getResponse.Cursor);

            return collection;
        }
    }
}