using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;
using AuthenticationManager = Usergrid.Sdk.Manager.AuthenticationManager;

namespace Usergrid.Sdk {
    public class Client : IClient {
        private const string UserGridEndPoint = "http://api.usergrid.com";
        private readonly IUsergridRequest _request;

        private IAuthenticationManager _authenticationManager;
        private IConnectionManager _connectionManager;
        private IEntityManager _entityManager;
        private INotificationsManager _notificationsManager;

        public Client(string organization, string application)
            : this(organization, application, UserGridEndPoint, new UsergridRequest(UserGridEndPoint, organization, application)) {}

        public Client(string organization, string application, string uri = UserGridEndPoint)
            : this(organization, application, uri, new UsergridRequest(uri, organization, application)) {}

        internal Client(string organization, string application, string uri = UserGridEndPoint, IUsergridRequest request = null) {
            _request = request ?? new UsergridRequest(uri, organization, application);
        }

        internal IAuthenticationManager AuthenticationManager {
            get { return _authenticationManager ?? (_authenticationManager = new AuthenticationManager(_request)); }
            set { _authenticationManager = value; }
        }

        internal IEntityManager EntityManager {
            get { return _entityManager ?? (_entityManager = new EntityManager(_request)); }
            set { _entityManager = value; }
        }

        internal IConnectionManager ConnectionManager {
            get { return _connectionManager ?? (_connectionManager = new ConnectionManager(_request)); }
            set { _connectionManager = value; }
        }

        internal INotificationsManager NotificationsManager {
            get { return _notificationsManager ?? (_notificationsManager = new NotificationsManager(_request)); }
            set { _notificationsManager = value; }
        }

        public async Task Login(string loginId, string secret, AuthType authType) {
            await AuthenticationManager.Login(loginId, secret, authType);
        }

        public async Task<T> CreateEntity<T>(string collection, T entity) {
            return await EntityManager.CreateEntity(collection, entity);
        }

        public async Task DeleteEntity(string collection, string name) {
            await EntityManager.DeleteEntity(collection, name);
        }

        public async Task UpdateEntity<T>(string collection, string identifier, T entity) {
            await EntityManager.UpdateEntity(collection, identifier, entity);
        }

        public async Task<T> GetEntity<T>(string collectionName, string entityIdentifier) {
            return await EntityManager.GetEntity<T>(collectionName, entityIdentifier);
        }

        public async Task<T> GetUser<T>(string identifer /*username or uuid or email*/) where T : UsergridUser {
            var user = await GetEntity<T>("users", identifer);
            return user;
        }

        public async Task CreateUser<T>(T user) where T : UsergridUser {
            await CreateEntity("users", user);
        }

        public async Task UpdateUser<T>(T user) where T : UsergridUser {
            await UpdateEntity("users", user.UserName, user);
        }

        public async Task DeleteUser(string identifer /*username or uuid or email*/) {
            await DeleteEntity("users", identifer);
        }

        public async Task ChangePassword(string identifer /*username or uuid or email*/, string oldPassword, string newPassword) {
            await AuthenticationManager.ChangePassword(identifer, oldPassword, newPassword);
        }

        public async Task CreateGroup<T>(T group) where T : UsergridGroup {
            await CreateEntity("groups", group);
        }

        public async Task DeleteGroup(string path) {
            await DeleteEntity("groups", path);
        }

        public async Task<T> GetGroup<T>(string identifer /*uuid or path*/) where T : UsergridGroup {
            var usergridGroup = await EntityManager.GetEntity<T>("groups", identifer);
            if (usergridGroup == null)
                return null;

            return usergridGroup;
        }

        public async Task UpdateGroup<T>(T group) where T : UsergridGroup {
            await UpdateEntity("groups", group.Path, group);
        }

        public async Task AddUserToGroup(string groupIdentifier, string userName) {
            await EntityManager.CreateEntity<object>(string.Format("/groups/{0}/users/{1}", groupIdentifier, userName), null);
        }

        public async Task DeleteUserFromGroup(string groupIdentifier, string userIdentifier) {
            await DeleteEntity("/groups/" + groupIdentifier + "/users", userIdentifier);
        }

        public async Task<IList<T>> GetAllUsersInGroup<T>(string groupName) where T : UsergridUser {
            IRestResponse response = await _request.ExecuteJsonRequest(string.Format("/groups/{0}/users", groupName), HttpMethod.Get);
            ValidateResponse(response);

            var responseObject = JsonConvert.DeserializeObject<UsergridGetResponse<T>>(response.Content);
            return responseObject.Entities;
        }

        public async Task<UsergridCollection<T>> GetEntities<T>(string collection, int limit = 10, string query = null) {
            return await EntityManager.GetEntities<T>(collection, limit, query);
        }

        public async Task<UsergridCollection<T>> GetNextEntities<T>(string collection, string query = null) {
            return await EntityManager.GetNextEntities<T>(collection, query);
        }

        public async Task<UsergridCollection<T>> GetPreviousEntities<T>(string collection, string query = null) {
            return await EntityManager.GetPreviousEntities<T>(collection, query);
        }

        public async Task CreateConnection(Connection connection) {
            await ConnectionManager.CreateConnection(connection);
        }
        public async Task<IList<UsergridEntity>> GetConnections(Connection connection) {
            return await ConnectionManager.GetConnections(connection);
        }
        public async Task<IList<TConnectee>> GetConnections<TConnectee>(Connection connection) {
            return await ConnectionManager.GetConnections<TConnectee>(connection);
        }
        public async Task DeleteConnection(Connection connection) {
            await ConnectionManager.DeleteConnection(connection);
        }

        public async Task PostActivity<T>(string userIdentifier, T activity) where T : UsergridActivity {
            string collection = string.Format("/users/{0}/activities", userIdentifier);
            await EntityManager.CreateEntity(collection, activity);
        }

        public async Task PostActivityToGroup<T>(string groupIdentifier, T activity) where T : UsergridActivity {
            string collection = string.Format("/groups/{0}/activities", groupIdentifier);
            await EntityManager.CreateEntity(collection, activity);
        }

        public async Task PostActivityToUsersFollowersInGroup<T>(string userIdentifier, string groupIdentifier, T activity) where T : UsergridActivity {
            string collection = string.Format("/groups/{0}/users/{1}/activities", groupIdentifier, userIdentifier);
            await EntityManager.CreateEntity(collection, activity);
        }

        public async Task<UsergridCollection<T>> GetUserActivities<T>(string userIdentifier) where T : UsergridActivity {
            string collection = string.Format("/users/{0}/activities", userIdentifier);
            return await EntityManager.GetEntities<T>(collection);
        }

        public async Task<UsergridCollection<T>> GetGroupActivities<T>(string groupIdentifier) where T : UsergridActivity {
            string collection = string.Format("/groups/{0}/activities", groupIdentifier);
            return await EntityManager.GetEntities<T>(collection);
        }

        public async Task<T> GetDevice<T>(string identifer) where T : UsergridDevice {
            var device = await GetEntity<T>("devices", identifer);
            if (device == null)
                return null;

            return device;
        }

        public async Task UpdateDevice<T>(T device) where T : UsergridDevice {
            await UpdateEntity("devices", device.Name, device);
        }

        public async Task CreateDevice<T>(T device) where T : UsergridDevice {
            await CreateEntity("devices", device);
        }

        public async Task DeleteDevice(string identifer) {
            await DeleteEntity("devices", identifer);
        }

        public async Task CreateNotifierForApple(string notifierName, string environment, byte[] p12Certificate) {
            await NotificationsManager.CreateNotifierForApple(notifierName, environment, p12Certificate);
        }

        public async Task CreateNotifierForAndroid(string notifierName, string apiKey) {
            await NotificationsManager.CreateNotifierForAndroid(notifierName, apiKey);
        }

        public async Task<T> GetNotifier<T>(string identifer /*uuid or notifier name*/) where T : UsergridNotifier {
            var usergridNotifier = await EntityManager.GetEntity<T>("/notifiers", identifer);
            return usergridNotifier;
        }

        public async Task DeleteNotifier(string notifierName) {
            await EntityManager.DeleteEntity("/notifiers", notifierName);
        }

        public async Task PublishNotification(IEnumerable<Notification> notifications, INotificationRecipients recipients, NotificationSchedulerSettings schedulerSettings = null) {
            await NotificationsManager.PublishNotification(notifications, recipients, schedulerSettings);
        }

        public async Task CancelNotification(string notificationIdentifier) {
            await EntityManager.UpdateEntity("/notifications", notificationIdentifier, new CancelNotificationPayload {Canceled = true});
        }

        //TODO: IList?
        public async Task<UsergridCollection<T>> GetUserFeed<T>(string userIdentifier) where T : UsergridActivity {
            string collection = string.Format("/users/{0}/feed", userIdentifier);
            return await EntityManager.GetEntities<T>(collection);
        }

        public async Task<UsergridCollection<T>> GetGroupFeed<T>(string groupIdentifier) where T : UsergridActivity {
            string collection = string.Format("/groups/{0}/feed", groupIdentifier);
            return await EntityManager.GetEntities<T>(collection);
        }


        private static void ValidateResponse(IRestResponse response) {
            if (response.StatusCode != HttpStatusCode.OK) {
                var userGridError = JsonConvert.DeserializeObject<UsergridError>(response.Content);
                throw new UsergridException(userGridError);
            }
        }
    }
}