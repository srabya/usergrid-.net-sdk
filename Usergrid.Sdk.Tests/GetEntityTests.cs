using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class GetEntityTests
    {
        [Test]
        public void ShouldGetToCorrectEndPoint()
        {
            var restResponseContent = new UsergridGetResponse<Friend> { Entities = new List<Friend>(), Cursor = "" };
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string entityName = "entity";

            var client = new Client(null, null, request: request);
            client.GetEntity<Friend>(collectionName, entityName);

            request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}/{1}", collectionName, entityName)),
                Arg.Is(HttpMethod.Get),
                Arg.Any<object>());
        }
        
        [Test]
        public async void ShouldReturnEntityCorrectly()
        {
            var friend = new Friend { Name = "name", Age = 1 };

            var restResponseContent = new UsergridGetResponse<Friend> { Entities = new List<Friend> { friend }, Cursor = "cursor" };
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string entityName = "entity";

            var client = new Client(null, null, request: request);
            var returnedFriend = await client.GetEntity<Friend>(collectionName, entityName);

            Assert.IsNotNull(returnedFriend);
            Assert.AreEqual(friend.Name, returnedFriend.Name);
            Assert.AreEqual(friend.Age, returnedFriend.Age);
        }

        [Test]
        public async void ShouldReturnFirstEntityInListCorrectly()
        {
            var friend1 = new Friend { Name = "name1", Age = 1 };
            var friend2 = new Friend { Name = "name2", Age = 2 };

            var entities = new List<Friend> { friend1, friend2 };
            var restResponseContent = new UsergridGetResponse<Friend> { Entities = entities, Cursor = "cursor" };

            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string entityName = "entity";

            var client = new Client(null, null, request: request);
            var returnedFriend = await client.GetEntity<Friend>(collectionName, entityName);

            Assert.IsNotNull(returnedFriend);
            Assert.AreEqual(friend1.Name, returnedFriend.Name);
            Assert.AreEqual(friend1.Age, returnedFriend.Age);
        }

        [Test]
        public async void ShouldReturnNullEntityCorrectly()
        {
            var entities = new List<Friend> { null };

            var restResponseContent = new UsergridGetResponse<Friend> { Entities = entities, Cursor = "cursor" };
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string entityName = "entity";

            var client = new Client(null, null, request: request);
            var returnedFriend = await client.GetEntity<Friend>(collectionName, entityName);

            Assert.IsNull(returnedFriend);
        }
    }
}