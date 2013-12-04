using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Manager
{
    internal class NotificationsManager : ManagerBase, INotificationsManager
    {
        public NotificationsManager(IUsergridRequest request) : base(request)
        {
        }

        public async Task CreateNotifierForApple(string notifierName, string environment, byte[] p12Certificate) {
            var payload = new AppleNotifierPayload()
                {
                    Name = notifierName,
                    Environment = environment,
                    p12Certificate = p12Certificate
                };
            IRestResponse response = await Request.ExecuteJsonRequest("/notifiers", HttpMethod.Post, payload);
            ValidateResponse(response);
        }

        public async Task CreateNotifierForAndroid(string notifierName, string apiKey)
        {
            IRestResponse response = await Request.ExecuteJsonRequest("/notifiers", HttpMethod.Post, new AndroidNotifierPayload {ApiKey = apiKey, Name = notifierName});
            ValidateResponse(response);
        }

        public async Task PublishNotification(IEnumerable<Notification> notifications, INotificationRecipients recipients, NotificationSchedulerSettings schedulerSettings = null)
        {
            var payload = new NotificationPayload();
            foreach (Notification notification in notifications)
            {
                payload.Payloads.Add(notification.NotifierIdentifier, notification.GetPayload());
            }

            if (schedulerSettings != null)
            {
                if (schedulerSettings.DeliverAt != DateTime.MinValue)
                    payload.DeliverAt = schedulerSettings.DeliverAt.ToUnixTime();
                if (schedulerSettings.ExpireAt != DateTime.MinValue)
                    payload.ExpireAt = schedulerSettings.ExpireAt.ToUnixTime();
            }

            string query = recipients.BuildQuery();

            IRestResponse response = await Request.ExecuteJsonRequest(query, HttpMethod.Post, payload);
            ValidateResponse(response);
        }
    }
}