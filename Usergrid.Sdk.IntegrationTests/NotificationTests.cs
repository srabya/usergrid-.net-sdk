using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    [TestFixture]
    public class NotificationTests : BaseTest {
        private static async void CreateUser(string username, IClient client) {
            var userEntity = new MyUsergridUser {UserName = username};
            // See if this user exists
            var userFromUsergrid = await client.GetUser<UsergridUser>(username);
            // Delete if exists
            if (userFromUsergrid != null) {
                await client.DeleteUser(username);
            }
            // Now create the user
            await client.CreateUser(userEntity);
        }


        private async Task CreateAppleNotifier(IClient client, string notifierName) {
            await DeleteNotifierIfExists(client, notifierName);
            await client.CreateNotifierForApple(notifierName, "development", File.ReadAllBytes(P12CertificatePath));
        }

        private async Task CreateAndroidNotifier(IClient client, string notifierName) {
            await DeleteNotifierIfExists(client, notifierName);
            await client.CreateNotifierForAndroid(notifierName, GoogleApiKey);
        }

        private static async Task DeleteNotifierIfExists(IClient client, string notifierName) {
            var usergridNotifier = await client.GetNotifier<UsergridNotifier>(notifierName);
            if (usergridNotifier != null)
                await client.DeleteNotifier(usergridNotifier.Uuid);
        }

        [Test]
        public async void ShouldCreateNotifierForAndroid() {
            const string notifierName = "test_notifier_new";
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteNotifierIfExists(client, notifierName);

            await client.CreateNotifierForAndroid(notifierName, GoogleApiKey /*e.g. AIzaSyCkXOtBQ7A9GoJsSLqZlod_YjEfxxxxxxx*/);

            UsergridNotifier usergridNotifier = await client.GetNotifier<UsergridNotifier>(notifierName);
            Assert.That(usergridNotifier, Is.Not.Null);
            Assert.That(usergridNotifier.Provider, Is.EqualTo("google"));
            Assert.That(usergridNotifier.Name, Is.EqualTo(notifierName));
        }

        [Test]
        public async void ShouldCreateNotifierForApple() {
            const string notifierName = "test_notifier";
            const string environment = "development";
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteNotifierIfExists(client, notifierName);

            await client.CreateNotifierForApple(notifierName, environment, File.ReadAllBytes(P12CertificatePath));

            UsergridNotifier usergridNotifier = await client.GetNotifier<UsergridNotifier>(notifierName);

            Assert.That(usergridNotifier, Is.Not.Null);
            Assert.That(usergridNotifier.Environment, Is.EqualTo(environment));
            Assert.That(usergridNotifier.Provider, Is.EqualTo("apple"));
            Assert.That(usergridNotifier.Name, Is.EqualTo(notifierName));
        }

        [Test]
        public async void ShouldPublishNotifications() {
            //Set up
            const string appleNotifierName = "apple_notifier";
            const string googleNotifierName = "google_notifier";
            const string username = "NotificationTestUser";
            const string appleTestMessge = "test message for Apple";
            const string androidTestMessage = "test message for Android";

            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            await CreateAppleNotifier(client, appleNotifierName);
            await CreateAndroidNotifier(client, googleNotifierName);
            CreateUser(username, client);

            //Setup Notifications
            var appleNotification = new AppleNotification(appleNotifierName, appleTestMessge, "chime");
            var googleNotification = new AndroidNotification(googleNotifierName, androidTestMessage);
            //Setup recipients and scheduling
            INotificationRecipients recipients = new NotificationRecipients().AddUserWithName(username);
            var schedulerSettings = new NotificationSchedulerSettings {DeliverAt = DateTime.Now.AddDays(1)};

            await client.PublishNotification(new Notification[] {appleNotification, googleNotification}, recipients, schedulerSettings);

            //Assert
            UsergridCollection<dynamic> entities = await client.GetEntities<dynamic>("notifications", query: "order by created desc");
            dynamic notification = entities.FirstOrDefault();

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.uuid);
            Assert.AreEqual(appleTestMessge, notification.payloads.apple_notifier.aps.alert.Value);
            Assert.AreEqual("chime", notification.payloads.apple_notifier.aps.sound.Value);
            Assert.AreEqual(androidTestMessage, notification.payloads.google_notifier.data.Value);

            //Cancel notification and assert it is canceled
            await (Task) client.CancelNotification(notification.uuid.Value);
            dynamic entity = await (Task<dynamic>) client.GetEntity<dynamic>("notifications", notification.uuid.Value);
            Assert.AreEqual(entity.state.Value, "CANCELED");
        }
    }
}