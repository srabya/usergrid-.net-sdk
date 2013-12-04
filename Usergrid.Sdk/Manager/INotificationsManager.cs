using System.Collections.Generic;
using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Manager
{
    internal interface INotificationsManager
    {
        Task CreateNotifierForApple(string notifierName, string environment, byte[] p12Certificate);
        Task CreateNotifierForAndroid(string notifierName, string apiKey);
		Task PublishNotification (IEnumerable<Notification> notification, INotificationRecipients recipients, NotificationSchedulerSettings schedulingSettings = null);
    }
}