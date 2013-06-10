using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
    [TestFixture]
    public class NotificationTests : BaseTest
    {
        [Test]
        public void ShouldCreateNotifierForAndroid()
        {
            const string notifierName = "test_notifier";
            var client = InitializeClientAndLogin(AuthType.ClientId);
            DeleteNotifierIfExists(client, notifierName);

            client.CreateNotifierForAndroid(notifierName, GoogleApiKey);

            UsergridNotifier usergridNotifier = client.GetNotifier<UsergridNotifier>(notifierName);
            Assert.That(usergridNotifier, Is.Not.Null);
            Assert.That(usergridNotifier.Provider, Is.EqualTo("google"));
            Assert.That(usergridNotifier.Name, Is.EqualTo(notifierName));
        }

        [Test]
        public void ShouldCreateNotifierForApple()
        {
            const string notifierName = "test_notifier";
            const string environment = "development";
            var client = InitializeClientAndLogin(AuthType.ClientId);
            DeleteNotifierIfExists(client, notifierName);

            client.CreateNotifierForApple(notifierName, environment, base.P12CertificatePath);

            UsergridNotifier usergridNotifier = client.GetNotifier<UsergridNotifier>(notifierName);

            Assert.That(usergridNotifier, Is.Not.Null);
            Assert.That(usergridNotifier.Environment, Is.EqualTo(environment));
            Assert.That(usergridNotifier.Provider, Is.EqualTo("apple"));
            Assert.That(usergridNotifier.Name, Is.EqualTo(notifierName));
        }

        [Test]
        public void ShouldPublishNotifications()
        {
            //Set up
            const string appleNotifierName = "apple_notifier";
            const string googleNotifierName = "google_notifier";
            const string username = "NotificationTestUser";
            const string appleTestMessge = "test message for Apple";
            const string androidTestMessage = "test message for Android";

            var client = InitializeClientAndLogin(AuthType.ClientId);
            CreateAppleNotifier(client,appleNotifierName);            
            CreateAndroidNotifier(client,googleNotifierName);            
            CreateUser(username, client);

            //Setup Notifications
            var appleNotification = new AppleNotification(appleNotifierName, appleTestMessge, "chime");
            var googleNotification = new AndroidNotification(googleNotifierName, androidTestMessage);
            //Setup recipients and scheduling
            INotificationRecipients recipients = new NotificationRecipients().AddUserWithName(username);
            var schedulerSettings = new NotificationSchedulerSettings {DeliverAt = DateTime.Now.AddDays(1)};

            client.PublishNotification(new Notification[] {appleNotification, googleNotification}, recipients, schedulerSettings);

            //Assert
            UsergridCollection<UsergridEntity<dynamic>> entities = client.GetEntities<dynamic>("notifications", query: "order by created desc");
            UsergridEntity<dynamic> notification = entities.FirstOrDefault();

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.Uuid);
            Assert.AreEqual(appleTestMessge, notification.Entity.payloads.apple_notifier.aps.alert.Value);
            Assert.AreEqual("chime", notification.Entity.payloads.apple_notifier.aps.sound.Value);
            Assert.AreEqual(androidTestMessage, notification.Entity.payloads.google_notifier.data.Value);

            //Cancel notification and assert it is canceled
            client.CancelNotification(notification.Uuid);
            UsergridEntity<dynamic> entity = client.GetEntity<dynamic>("notifications", notification.Uuid);
            Assert.AreEqual(entity.Entity.state.Value, "CANCELED");
        }

        private static void CreateUser(string username, IClient client)
        {
            var userEntity = new MyUsergridUser {UserName = username};
            // See if this user exists
            var userFromUsergrid = client.GetUser<UsergridUser>(username);
            // Delete if exists
            if (userFromUsergrid != null) {
                client.DeleteUser(username);
            }
            // Now create the user
            client.CreateUser(userEntity);
        }


        private void CreateAppleNotifier(IClient client, string notifierName)
        {
            DeleteNotifierIfExists(client, notifierName);
            client.CreateNotifierForApple(notifierName, "development", base.P12CertificatePath);
        }

        private void CreateAndroidNotifier(IClient client, string notifierName)
        {
            DeleteNotifierIfExists(client, notifierName);
            client.CreateNotifierForAndroid(notifierName, GoogleApiKey);
        }
        private static void DeleteNotifierIfExists(IClient client, string notifierName)
        {
            var usergridNotifier = client.GetNotifier<UsergridNotifier>(notifierName);
            if (usergridNotifier != null)
                client.DeleteNotifier(usergridNotifier.Uuid);
        }

    }
}