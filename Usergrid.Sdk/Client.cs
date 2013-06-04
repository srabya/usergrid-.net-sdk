using System;
//using System.Net;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;
using System.Collections.Generic;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk
{
    public class Client
    {
        private const string UserGridEndPoint = "https://api.usergrid.com";
        private readonly IUsergridRequest _request;

        private IEntityManager _entityManager;
		private IAuthenticationManager _authenticationManager;
		private IConnectionManager _connectionManager;

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return _authenticationManager ?? (_authenticationManager = new AuthenticationManager (_request));
			}
		}

		private IEntityManager EntityManager 
		{
			get
			{
				return _entityManager ?? (_entityManager = new EntityManager (_request));
			}
		}

		private IConnectionManager ConnectionManager
		{
			get
			{
				return _connectionManager ?? (_connectionManager = new ConnectionManager (_request));
			}
		}

        public Client(string organization, string application)
            : this(organization, application, UserGridEndPoint, new UsergridRequest(UserGridEndPoint, organization, application))
        {
        }

        public Client(string organization, string application, string uri = UserGridEndPoint)
            : this(organization, application, uri, new UsergridRequest(uri, organization, application))
        {
        }

        internal Client(string organization, string application, string uri = UserGridEndPoint, IUsergridRequest request = null)
        {
            _request = request ?? new UsergridRequest(uri, organization, application);
        }

        public void Login(string loginId, string secret, AuthType authType)
        {
			AuthenticationManager.Login (loginId, secret, authType);
        }

        public void CreateEntity<T>(string collection, T entity = null) where T : class
        {
            EntityManager.CreateEntity(collection,entity);
        }

        public void DeleteEntity(string collection, string name)
        {
			EntityManager.DeleteEntity(collection, name);
        }

        public void UpdateEntity<T>(string collection, string identifier, T entity)
        {
			EntityManager.UpdateEntity (collection, identifier, entity);
        }

        public UsergridEntity<T> GetEntity<T>(string collectionName, string identifer)
        {
		    return EntityManager.GetEntity<T>(collectionName, identifer);
		}

        public T GetUser<T>(string identifer /*username or uuid or email*/) where T : UsergridUser
        {
			var user = GetEntity<T> ("users", identifer);
			if (user == null)
				return null;

			return user.Entity;
        }

        public void CreateUser<T>(T user) where T : UsergridUser
        {
            CreateEntity("users", user);
        }

		public void UpdateUser<T>(T user) where T : UsergridUser
		{
            UpdateEntity("users", user.UserName, user);
		}

        public void DeleteUser(string identifer /*username or uuid or email*/)
		{
            DeleteEntity("users", identifer);
		}

        public void ChangePassword(string userName, string oldPassword, string newPassword)
		{
			AuthenticationManager.ChangePassword (userName, oldPassword, newPassword);
		}

		public void CreateGroup<T>(T group) where T : UsergridGroup
		{
			CreateEntity ("groups", group);
		}

		public void DeleteGroup(string path)
		{
			DeleteEntity ("groups", path);
		}

        public T GetGroup<T>(string identifer/*uuid or path*/) where T : UsergridGroup
		{
            var usergridEntity = EntityManager.GetEntity<T>("groups", identifer);
			if (usergridEntity == null)
				return null;

            return usergridEntity.Entity; 
		}

		public void UpdateGroup<T>(T group) where T : UsergridGroup
		{
			UpdateEntity<T> ("groups", group.Path, group);
		}

        public void AddUserToGroup(string groupName, string userName)
        {
			CreateEntity<object> (string.Format("/groups/{0}/users/{1}", groupName, userName));
		}

        public void DeleteUserFromGroup(string groupName, string userName)
        {
            DeleteEntity("groups/" + groupName + "/users", userName );
		}

        public IList<T> GetAllUsersInGroup<T>(string groupName) where T : UsergridUser
        {
            var response = _request.Execute(string.Format("/groups/{0}/users", groupName) , Method.GET);
            ValidateResponse(response);

            var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            return responseObject.Entities;
        }

		public UsergridCollection<UsergridEntity<T>> GetEntities<T>(string collection, int limit)
		{
			return EntityManager.GetEntities<T>(collection, limit);
		}

		public UsergridCollection<UsergridEntity<T>> GetNextEntities<T>(string collection)
		{
			return EntityManager.GetNextEntities<T> (collection);
		}

		public UsergridCollection<UsergridEntity<T>> GetPreviousEntities<T>(string collection)
		{
			return EntityManager.GetPreviousEntities<T> (collection);
		}

		public void CreateConnection<TConnector, TConnectee> (TConnector connector, TConnectee connectee, string connection) where TConnector : Usergrid.Sdk.Model.UsergridEntity where TConnectee : Usergrid.Sdk.Model.UsergridEntity
        {
			ConnectionManager.CreateConnection (connector, connectee, connection);
        }

		public IList<UsergridEntity> GetConnections<TConnector> (TConnector connector, string connection) where TConnector : Usergrid.Sdk.Model.UsergridEntity
		{
			return ConnectionManager.GetConnections<TConnector> (connector, connection);
		}

		public IList<UsergridEntity<TConnectee>> GetConnections<TConnector, TConnectee> (TConnector connector, string connection) where TConnector : Usergrid.Sdk.Model.UsergridEntity where TConnectee : Usergrid.Sdk.Model.UsergridEntity
		{
			return ConnectionManager.GetConnections<TConnector, TConnectee> (connector, connection);
		}

		public void DeleteConnection<TConnector, TConnectee> (TConnector connector, TConnectee connectee, string connection) where TConnector : Usergrid.Sdk.Model.UsergridEntity where TConnectee : Usergrid.Sdk.Model.UsergridEntity
		{
			ConnectionManager.DeleteConnection (connector, connectee, connection);
		}

        private static void ValidateResponse(IRestResponse response)
		{
			if (response.StatusCode != System.Net.HttpStatusCode.OK) {
				var userGridError = JsonConvert.DeserializeObject<UsergridError> (response.Content);
				throw new UsergridException (userGridError);
			}
		}
    }
}