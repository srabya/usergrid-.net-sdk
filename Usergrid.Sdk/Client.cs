using System;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;
using System.Collections.Generic;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk
{
    public class Client
    {
        private const string UserGridEndPoint = "https://api.usergrid.com";
        private readonly IUsergridRequest _request;
		private readonly IDictionary<Type, IList<string>> _cursors = new Dictionary<Type, IList<string>>();
		private readonly IDictionary<Type, int> _cursorPositions = new Dictionary<Type, int> ();

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
            if (authType == AuthType.None)
            {
                AccessToken = null;
                return;
            }

            var body = GetLoginBody(loginId, secret, authType);

            var response = _request.Execute<LoginResponse>("/token", Method.POST, body);
            ValidateResponse(response);

            AccessToken = response.Data.AccessToken;
        }

        private object GetLoginBody(string loginId, string secret, AuthType authType)
        {
            object body = null;

            if (authType == AuthType.ClientId || authType == AuthType.Application)
            {
                body = new ClientIdLoginPayload
                {
                    ClientId = loginId,
                    ClientSecret = secret
                };
            }
            else if (authType == AuthType.User)
            {
                body = new UserLoginPayload
                {
                    UserName = loginId,
                    Password = secret
                };
            }
            return body;
        }

        public void CreateEntity<T>(string collection, T entity = null) where T : class
        {
            IRestResponse response = _request.Execute("/" + collection, Method.POST, entity, AccessToken);
            ValidateResponse(response);
        }

        public void DeleteEntity(string collection, string name)
        {
			IRestResponse response = _request.Execute(string.Format("/{0}/{1}", collection, name), Method.DELETE, accessToken: AccessToken);
            ValidateResponse(response);
        }

        public void UpdateEntity<T>(string collection, string name, T entity)
        {
			dynamic response = _request.Execute(string.Format("/{0}/{1}", collection, name), Method.PUT, entity, AccessToken);
            ValidateResponse(response);
        }

		public T GetEntity<T>(string collection, string name)
		{
			var response = _request.Execute (string.Format ("/{0}/{1}", collection, name), Method.GET, accessToken: AccessToken);

			if (response.StatusCode == HttpStatusCode.NotFound)
				return default(T);
			ValidateResponse (response);

			var entity =  JsonConvert.DeserializeObject<UsergridGetResponse<T>> (response.Content);

			return entity.Entities.FirstOrDefault ();
		}

		public T GetUser<T> (string userName) where T : User
		{
			return GetEntity<T> ("users", userName);
		}

		public T GetUserByEmail<T> (string email) where T : User
		{
			return GetEntity<T> ("users", email);
		}

		public void CreateUser<T>(T user) where T : User
		{
			this.CreateEntity ("users", user);
		}

		public void UpdateUser<T>(T user) where T : User
		{
			UpdateEntity("users", user.UserName, user);
		}

		public void DeleteUser(string userName)
		{
			DeleteEntity ("users", userName);
		}

        public void ChangePassword(string userName, string oldPassword, string newPassword)
		{
            var payload = new ChangePasswordPayload {OldPassword = oldPassword, NewPassword = newPassword};
            var response = _request.Execute(string.Format("/users/{0}/password", userName), Method.POST, payload);
            ValidateResponse(response);
		}

		public void CreateGroup<T>(T group) where T : Group
		{
			CreateEntity ("groups", group);
		}

		public void DeleteGroup(string path)
		{
			DeleteEntity ("groups", path);
		}

		public T GetGroup<T>(string path) where T : Group
		{
			return GetEntity<T> ("groups", path);
		}

		public void UpdateGroup<T>(T group) where T : Group
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

        public IList<T> GetAllUsersInGroup<T>(string groupName) where T : User
        {
            var response = _request.Execute(string.Format("/groups/{0}/users", groupName) , Method.GET, accessToken: AccessToken);
            ValidateResponse(response);

            var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            return responseObject.Entities;
        }

		public IList<T> GetEntities<T>(string collection)
		{
			var response = _request.Execute (string.Format("/{0}", collection), Method.GET, accessToken: AccessToken);
			ValidateResponse (response);

			var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>> (response.Content);

			int currentCursorPosition = 0;
			if (_cursors.ContainsKey(typeof(T))){
				IList<string> thisTypesCursors = _cursors [typeof(T)];
				thisTypesCursors [0] = null;
				thisTypesCursors [++currentCursorPosition] = responseObject.Cursor;
			} else {
				var cursorsForThisType = new List<string> ();
				cursorsForThisType.Add (null);
				cursorsForThisType.Add (responseObject.Cursor);
				_cursors.Add (typeof(T), cursorsForThisType);
			}

			if (_cursorPositions.ContainsKey(typeof(T))){
				_cursorPositions[typeof(T)] = currentCursorPosition;
			} else {
				_cursorPositions.Add (typeof(T), currentCursorPosition);
			}

			return responseObject.Entities;
		}

		public IList<T> GetNextEntities<T>(string collection)
		{
			// get previous cursor position
			int previousCursorPosition = _cursorPositions [typeof(T)];
			int nextCursorPosition = ++previousCursorPosition;

			// get next cursor
			string cursor = _cursors [typeof(T)] [nextCursorPosition];

			var response = _request.Execute (string.Format("/{0}?cursor={1}", collection, cursor), Method.GET, accessToken: AccessToken);
			ValidateResponse (response);

			var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>> (response.Content);

			_cursors [typeof(T)].Add (responseObject.Cursor);
			_cursorPositions[typeof(T)] = nextCursorPosition;

			return responseObject.Entities;
		}

		public IList<T> GetPreviousEntities<T>(string collection)
		{
			// get current cursor position
			int previousCursorPosition = _cursorPositions [typeof(T)];
			int nextCursorPosition = --previousCursorPosition;

			// get current cursor
			string cursor = _cursors [typeof(T)] [nextCursorPosition];

			var response = _request.Execute (string.Format("/{0}?cursor={1}", collection, cursor), Method.GET, accessToken: AccessToken);
			ValidateResponse (response);

			var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>> (response.Content);

			_cursorPositions[typeof(T)] = nextCursorPosition;

			return responseObject.Entities;
		}

        public string AccessToken { get; private set; }

        private static void ValidateResponse(IRestResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var userGridError = JsonConvert.DeserializeObject<UsergridError>(response.Content);
                throw new UsergridException(userGridError);
            }
        }
    }
}