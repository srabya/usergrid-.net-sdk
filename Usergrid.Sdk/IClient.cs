using System.Collections.Generic;
using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk
{
    public interface IClient
    {
        Task Login(string loginId, string secret, AuthType authType);
        Task<T> CreateEntity<T>(string collection, T entity);
        Task DeleteEntity(string collection, string name);
        Task UpdateEntity<T>(string collection, string identifier, T entity);
        Task<T> GetEntity<T>(string collectionName, string identifer);
        
        Task<T> GetUser<T>(string identifer /*username or uuid or email*/) where T : UsergridUser;
        Task CreateUser<T>(T user) where T : UsergridUser;
        Task UpdateUser<T>(T user) where T : UsergridUser;
        Task DeleteUser(string identifer /*username or uuid or email*/);
        Task ChangePassword(string identifer /*username or uuid or email*/, string oldPassword, string newPassword);
        Task CreateGroup<T>(T group) where T : UsergridGroup;
        Task DeleteGroup(string path);
        Task<T> GetGroup<T>(string identifer /*uuid or path*/) where T : UsergridGroup;
        Task UpdateGroup<T>(T group) where T : UsergridGroup;
        Task AddUserToGroup(string groupIdentifier, string userName);
        Task DeleteUserFromGroup(string groupIdentifier, string userIdentifier);
        Task<IList<T>> GetAllUsersInGroup<T>(string groupName) where T : UsergridUser;
        
        Task<UsergridCollection<T>> GetEntities<T>(string collection, int limit = 10, string query = null);
        Task<UsergridCollection<T>> GetNextEntities<T>(string collection, string query = null);
        Task<UsergridCollection<T>> GetPreviousEntities<T>(string collection, string query = null);

        Task CreateConnection(Connection connection);
        Task<IList<UsergridEntity>> GetConnections(Connection connection);
        Task<IList<TConnectee>> GetConnections<TConnectee>(Connection connection);
        Task DeleteConnection(Connection connection);
        
        Task PostActivity<T>(string userIdentifier, T activity) where T:UsergridActivity;
        Task PostActivityToGroup<T>(string groupIdentifier, T activity) where T:UsergridActivity;
        Task PostActivityToUsersFollowersInGroup<T>(string userIdentifier, string groupIdentifier, T activity) where T:UsergridActivity;
        Task<UsergridCollection<T>> GetUserActivities<T>(string userIdentifier) where T:UsergridActivity;
        Task<UsergridCollection<T>> GetGroupActivities<T>(string groupIdentifier) where T:UsergridActivity;
        Task<UsergridCollection<T>> GetUserFeed<T>(string userIdentifier) where T : UsergridActivity;
        Task<UsergridCollection<T>> GetGroupFeed<T>(string groupIdentifier) where T : UsergridActivity;


        Task CreateNotifierForApple(string notifierName, string environment, byte[] p12Certificate);
        Task CreateNotifierForAndroid(string notifierName, string apiKey);
        Task<T> GetNotifier<T>(string identifer/*uuid or notifier name*/) where T : UsergridNotifier;
        Task DeleteNotifier(string notifierName);
        
        
        Task<T> GetDevice<T>(string identifer) where T : UsergridDevice;
        Task UpdateDevice<T>(T device) where T : UsergridDevice;
        Task CreateDevice<T>(T device) where T : UsergridDevice;
        Task DeleteDevice(string identifer);
        Task PublishNotification (IEnumerable<Notification> notifications, INotificationRecipients recipients, NotificationSchedulerSettings schedulerSettings = null );
        Task CancelNotification(string notificationIdentifier);
    }
}