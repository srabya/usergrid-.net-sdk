using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;
using System.Collections.Generic;

namespace Usergrid.Sdk.Manager
{
    internal class EntityManager : ManagerBase, IEntityManager
    {
		private IDictionary<Type, Stack<string>> _cursorStates = new Dictionary<Type, Stack<string>> ();
		private IDictionary<Type, int> _pageSizes = new Dictionary<Type, int> ();

		internal EntityManager(IUsergridRequest request) : base(request) {}

        public void CreateEntity<T>(string collection, T entity = default(T)) where T : class
        {
            IRestResponse response = Request.Execute("/" + collection, Method.POST, entity);
            ValidateResponse(response);
        }

        public void DeleteEntity(string collection, string identifer)
        {
            IRestResponse response = Request.Execute(string.Format("/{0}/{1}", collection, identifer), Method.DELETE);
            ValidateResponse(response);
        }

        public void UpdateEntity<T>(string collection, string identifer, T entity)
        {
            IRestResponse response = Request.Execute(string.Format("/{0}/{1}", collection, identifer), Method.PUT, entity);
            ValidateResponse(response);
        }

        public UsergridEntity<T> GetEntity<T>(string collectionName, string identifer)
        {
            IRestResponse response = Request.Execute(string.Format("/{0}/{1}", collectionName, identifer), Method.GET);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return default(UsergridEntity<T>);
            ValidateResponse(response);

			var settings = new JsonSerializerSettings ();
			settings.Converters.Add (new EntitySerializer());
            var entity = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity<T>>>(response.Content, settings);

            return entity.Entities.FirstOrDefault();
        }

		public UsergridCollection<UsergridEntity<T>> GetEntities<T>(string collectionName, int limit = 10, string query = null)
		{
			_pageSizes.Remove (typeof(T));
			_pageSizes.Add (typeof(T), limit);

			var url = string.Format ("/{0}?limit={1}", collectionName, limit);
			if (query != null)
				url += "&query=" + query;

			IRestResponse response = Request.Execute(url, Method.GET);

			if (response.StatusCode == HttpStatusCode.NotFound)
				return new UsergridCollection<UsergridEntity<T>> ();

			ValidateResponse(response);

			var settings = new JsonSerializerSettings ();
			settings.Converters.Add (new EntitySerializer());
			var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity<T>>> (response.Content, settings);
			var collection = new UsergridCollection<UsergridEntity<T>> (getResponse.Entities);

			_cursorStates.Remove (typeof(T));

			if (getResponse.Cursor != null)
			{
				collection.HasNext = true;
				collection.HasPrevious = false;
				_cursorStates.Add (typeof(T), new Stack<string> ( new[] {null, null, getResponse.Cursor }));
			}

			return collection;
		}

		public UsergridCollection<UsergridEntity<T>> GetNextEntities<T>(string collectionName, string query = null)
		{
			if (!_cursorStates.ContainsKey(typeof(T)))
			{
				return new UsergridCollection<UsergridEntity<T>> ();
			}

			var stack = _cursorStates[typeof(T)];
			var cursor = stack.Peek ();

			if (cursor == null)
			{
				return new UsergridCollection<UsergridEntity<T>> () { HasNext = false, HasPrevious = true };
			}

			var limit = _pageSizes [typeof(T)];

			var url = string.Format("/{0}?cursor={1}&limit={2}", collectionName, cursor, limit);
			if (query != null)
				url += "&query=" + query;

			IRestResponse response = Request.Execute(url, Method.GET);

			if (response.StatusCode == HttpStatusCode.NotFound)
				return new UsergridCollection<UsergridEntity<T>> ();

			ValidateResponse (response);

			var settings = new JsonSerializerSettings ();
			settings.Converters.Add (new EntitySerializer());
			var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity<T>>> (response.Content, settings);
			var collection = new UsergridCollection<UsergridEntity<T>> (getResponse.Entities);

			if (getResponse.Cursor != null)
			{
				collection.HasNext = true;
				stack.Push (getResponse.Cursor);
			} else
			{
				stack.Push (null);
			}

			collection.HasPrevious = true;

			return collection;
		}

		public UsergridCollection<UsergridEntity<T>> GetPreviousEntities<T>(string collectionName, string query = null)
		{
			if (!_cursorStates.ContainsKey(typeof(T)))
			{
				var error = new UsergridError {
					Error = "cursor_not_initialized",
					Description = "Call GetEntities method to initialize the cursor"
				};
				throw new UsergridException (error);
			}

			var stack = _cursorStates[typeof(T)];
			stack.Pop ();
			stack.Pop ();
			var cursor = stack.Peek ();

			var limit = _pageSizes [typeof(T)];

			if (cursor == null)
			{
				return GetEntities<T> (collectionName, limit);
			}

			var url = string.Format("/{0}?cursor={1}&limit={2}", collectionName, cursor, limit);
			if (query != null)
				url += "&query=" + query;

			IRestResponse response = Request.Execute (url, Method.GET);

			if (response.StatusCode == HttpStatusCode.NotFound)
				return new UsergridCollection<UsergridEntity<T>> ();

			ValidateResponse (response);

			var settings = new JsonSerializerSettings ();
			settings.Converters.Add (new EntitySerializer());
			var getResponse = JsonConvert.DeserializeObject<UsergridGetResponse<UsergridEntity<T>>> (response.Content, settings);
			var collection = new UsergridCollection<UsergridEntity<T>> (getResponse.Entities);

			collection.HasNext = true;
			collection.HasPrevious = true;
			stack.Push (getResponse.Cursor);

			return collection;
		}
    }
}