using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests.ClientTests
{
    [TestFixture]
    public class GroupTests
    {
        [SetUp]
        public void Setup()
        {
            _entityManager = Substitute.For<IEntityManager>();
            _request = Substitute.For<IUsergridRequest>();
            _client = new Client(null, null, request: _request) {EntityManager = _entityManager};
        }

        private IEntityManager _entityManager;
        private IClient _client;
        private IUsergridRequest _request;

        [Test]
        public void AddUserToGroupShouldDelegateToEntityManagerrWithCorrectConnectionAndIdentifiers()
        {
            _client.AddUserToGroup("groupIdentifier", "userIdentifier");

            _entityManager.Received(1).CreateEntity<object>("/groups/groupIdentifier/users/userIdentifier", null);
        }

        [Test]
        public void CreateGroupShouldDelegateToEntityManagerWithCorrectCollectionNameAndUser()
        {
            var group = new UsergridGroup();

            _client.CreateGroup(group);

            _entityManager.Received(1).CreateEntity("groups", group);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void CreateGroupShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.CreateEntity("groups", Arg.Any<UsergridGroup>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.CreateGroup<UsergridGroup>(null);
        }

        [Test]
        public void DeleteGroupShouldDelegateToEntityManagerrWithCorrectCollectionNameAndIdentfier()
        {
            _client.DeleteGroup("groupPath");

            _entityManager.Received(1).DeleteEntity("groups", "groupPath");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void DeleteGroupShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.DeleteEntity("groups", Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.DeleteGroup(null);
        }

        [Test]
        public void DeleteUserFromGroupShouldDelegateToEntityManagerrWithCorrectConnectionAndIdentifier()
        {
            _client.DeleteUserFromGroup("groupIdentifier", "userIdentifier");

            _entityManager.Received(1).DeleteEntity("/groups/groupIdentifier/users", "userIdentifier");
        }

        [Test]
        public void GetGroupShouldDelegateToEntityManagerWithCorrectCollectionNameAndIdentifier()
        {
            _client.GetGroup<UsergridGroup>("identifier");

            _entityManager.Received(1).GetEntity<UsergridGroup>("groups", "identifier");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void GetGroupShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.GetEntity<UsergridGroup>("groups", Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.GetGroup<UsergridGroup>(null);
        }

        [Test]
        public async void GetGroupShouldReturnNullForUnexistingGroup() {
            UsergridUser user = null;
            _entityManager.GetEntity<UsergridUser>("groups", "identifier").Returns(x => Task.FromResult(user));

            var usergridGroup = await _client.GetGroup<UsergridGroup>("identifier");

            Assert.IsNull(usergridGroup);
        }

        [Test]
        public async void GetGroupShouldReturnUsergridGroup()
        {
            var usergridGroup = new UsergridGroup();
            _entityManager.GetEntity<UsergridGroup>("groups", "identifier").Returns(x => Task.FromResult(usergridGroup));

            var returnedGroup = await _client.GetGroup<UsergridGroup>("identifier");

            Assert.AreEqual(usergridGroup, returnedGroup);
        }

        [Test]
        public void UpdateGroupShouldDelegateToEntityManagerrWithCorrectCollectionNameAndGroupPathAsTheIdentifier()
        {
            var group = new UsergridGroup {Path = "groupPath"};

            _client.UpdateGroup(group);

            _entityManager.Received(1).UpdateEntity("groups", group.Path, group);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void UpdateGroupShouldPassOnTheException()
        {
            var group = new UsergridGroup {Path = "groupPath"};

            _entityManager
                .When(m => m.UpdateEntity("groups", group.Path, group))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.UpdateGroup(group);
        }

        [Test]
        public async void GetAllUsersInGroupShouldGetAllUsersInGroup()
        {
            var expectedUserList = new List<UsergridUser>() {new UsergridUser() {UserName = "userName", Name = "user1"}};
            var responseContent = new UsergridGetResponse<UsergridUser>() {Entities = expectedUserList};
            var restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, responseContent);
            
            _request.ExecuteJsonRequest("/groups/groupName/users", HttpMethod.Get).Returns(Task.FromResult(restResponse));

            var returnedUsers = await _client.GetAllUsersInGroup<UsergridUser>("groupName");

            _request.Received(1).ExecuteJsonRequest("/groups/groupName/users", HttpMethod.Get);
            Assert.AreEqual(1, returnedUsers.Count);
            Assert.AreEqual("userName", returnedUsers[0].UserName);
            Assert.AreEqual("user1", returnedUsers[0].Name);
        }

        [Test]
        public async void GetAllUsersInGroupWillThrowWhenBadRequest()
        {
            UsergridError error = new UsergridError() {Description = "exception description", Error = "error code"};
            var restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.BadRequest, error);
            
            _request.ExecuteJsonRequest("/groups/groupName/users", HttpMethod.Get).Returns(Task.FromResult(restResponse));

            try
            {
                await _client.GetAllUsersInGroup<UsergridUser>("groupName");
                Assert.Fail("Was expecting Usergrid exception to be thrown.");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual(error.Description, e.Message);
                Assert.AreEqual(error.Error, e.ErrorCode);
            }
        }
    }
}