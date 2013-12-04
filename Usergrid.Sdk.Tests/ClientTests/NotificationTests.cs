using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests.ClientTests
{
    [TestFixture]
    public class NotificationTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _notificationsManager = Substitute.For<INotificationsManager>();
            _entityManager = Substitute.For<IEntityManager>();
            _client = new Client(null, null) {NotificationsManager = _notificationsManager, EntityManager = _entityManager};
        }

        #endregion

        private INotificationsManager _notificationsManager;
        private IEntityManager _entityManager;
        private IClient _client;

        [Test]
        public void CancelNotificationShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.CancelNotification("notificationIdentifier");

            _entityManager.Received(1).UpdateEntity("/notifications", "notificationIdentifier", Arg.Is<CancelNotificationPayload>(p => p.Canceled));
        }

        [Test]
        public void CreateNotifierForAndroidShouldDelegateToNotificationsManager()
        {
            _client.CreateNotifierForAndroid("notifierName", "apiKey");

            _notificationsManager.Received(1).CreateNotifierForAndroid("notifierName", "apiKey");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void CreateNotifierForAndroidShouldPassOnTheException()
        {
            _notificationsManager
                .When(m => m.CreateNotifierForAndroid(Arg.Any<string>(), Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.CreateNotifierForAndroid(null, null);
        }

        [Test]
        public void CreateNotifierForAppleShouldDelegateToNotificationsManager()
        {
            var p12Certificate = new byte[0];
            _client.CreateNotifierForApple("notifierName", "development", p12Certificate);

            _notificationsManager.Received(1).CreateNotifierForApple("notifierName", "development", p12Certificate);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void CreateNotifierForAppleShouldPassOnTheException()
        {
            _notificationsManager
                .When(m => m.CreateNotifierForApple(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.CreateNotifierForApple(null, null, null);
        }

        [Test]
        public void DeleteNotifierShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.DeleteNotifier("notifierIdentifier");

            _entityManager.Received(1).DeleteEntity("/notifiers", "notifierIdentifier");
        }

        [Test]
        public void GetNotifierShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.GetNotifier<UsergridNotifier>("notifierIdentifier");

            _entityManager.Received(1).GetEntity<UsergridNotifier>("/notifiers", "notifierIdentifier");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void GetNotifierShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.GetEntity<UsergridNotifier>(Arg.Any<string>(), Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.GetNotifier<UsergridNotifier>(null);
        }

        [Test]
        public async void GetNotifierShouldReturnNullForUnexistingNotifier()
        {
            UsergridNotifier entity = null;

            _entityManager.GetEntity<UsergridNotifier>("/notifiers", "notifierIdentifier").Returns(x => Task.FromResult(entity));

            var returnedEntity = await _client.GetNotifier<UsergridNotifier>("notifierIdentifier");

            Assert.IsNull(returnedEntity);
        }

        [Test]
        public async void GetNotifierShouldReturnUsergridNotifierFromEntityManager() {
            var entity = new UsergridNotifier();
            _entityManager.GetEntity<UsergridNotifier>("/notifiers", "notifierIdentifier").Returns(x => Task.FromResult(entity));

            var returnedEntity = await _client.GetNotifier<UsergridNotifier>("notifierIdentifier");

            Assert.AreEqual(entity, returnedEntity);
        }

        [Test]
        public void PublishNotificationShouldDelegateToNotificationsManagerWithCorrectParameters()
        {
            var notifications = new Notification[] {new AppleNotification("notifierName", "message", "chime")};
            INotificationRecipients recipients = new NotificationRecipients().AddUserWithName("username");
            var schedulerSettings = new NotificationSchedulerSettings {DeliverAt = DateTime.Now.AddDays(1)};

            _client.PublishNotification(notifications, recipients, schedulerSettings);

            _notificationsManager.Received(1).PublishNotification(notifications, recipients, schedulerSettings);
        }
    }
}