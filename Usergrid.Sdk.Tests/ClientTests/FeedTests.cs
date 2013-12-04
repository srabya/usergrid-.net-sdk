using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests.ClientTests
{
    [TestFixture]
    public class FeedTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _entityManager = Substitute.For<IEntityManager>();
            _client = new Client(null, null) {EntityManager = _entityManager};
        }

        #endregion

        private IEntityManager _entityManager;
        private IClient _client;

        [Test]
        public async void GetGroupFeedShouldDelegateToEntityManagerWithCorrectEndpoint()
        {
            var usergridActivities = new UsergridCollection<UsergridActivity>();
            _entityManager.GetEntities<UsergridActivity>("/groups/groupIdentifier/feed").Returns(Task.FromResult(usergridActivities));

            UsergridCollection<UsergridActivity> returnedActivities = await _client.GetGroupFeed<UsergridActivity>("groupIdentifier");

            _entityManager.Received(1).GetEntities<UsergridActivity>("/groups/groupIdentifier/feed");
            Assert.AreEqual(usergridActivities, returnedActivities);
        }

        [Test]
        public async void GetUserFeedShouldDelegateToEntityManagerWithCorrectEndpoint()
        {
            var usergridActivities = new UsergridCollection<UsergridActivity>();
            _entityManager.GetEntities<UsergridActivity>("/users/userIdentifier/feed").Returns(Task.FromResult(usergridActivities));

            UsergridCollection<UsergridActivity> returnedActivities = await _client.GetUserFeed<UsergridActivity>("userIdentifier");

            _entityManager.Received(1).GetEntities<UsergridActivity>("/users/userIdentifier/feed");
            Assert.AreEqual(usergridActivities, returnedActivities);
        }
    }
}