using System.Collections.Generic;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class GetEntitiesTests
    {
        [Test]
        public void ShouldGetToCorrectEndPoint()
        {
            string restResponseContent = new UsergridGetResponse<Friend> {Entities = new List<Friend>(), Cursor = ""}.Serialize();

            var restResponse = Substitute.For<IRestResponse<UsergridGetResponse<Friend>>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>(), Arg.Any<string>())
                .Returns(restResponse);

            const string collectionName = "collection";

            var client = new Client(null, null, request: request);
            client.GetEntities<Friend>(collectionName);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}", collectionName)),
                Arg.Is(Method.GET),
                Arg.Any<object>(),
                Arg.Any<string>());
        }

        [Test]
        public void ShouldPassCorrectAccessToken()
        {
            const string accessToken = "access_token";
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);

            string restResponseContent = new UsergridGetResponse<Friend> {Entities = new List<Friend>(), Cursor = ""}.Serialize();

            var restResponse = Substitute.For<IRestResponse<UsergridGetResponse<Friend>>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(restResponseContent);

            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>(), Arg.Any<string>())
                .Returns(restResponse);

            const string collectionName = "collection";

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);
            client.GetEntities<Friend>(collectionName);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}", collectionName)),
                Arg.Is(Method.GET),
                Arg.Any<object>(),
                accessToken);
        }

        [Test]
        public void ShouldReturnListCorrectly()
        {
            var friend1 = new Friend {Name = "name1", Age = 1};
            var friend2 = new Friend {Name = "name2", Age = 2};

            var entities = new List<Friend> {friend1, friend2};
            string restResponseContent = new UsergridGetResponse<Friend> {Entities = entities, Cursor = "cursor"}.Serialize();

            var restResponse = Substitute.For<IRestResponse<UsergridGetResponse<Friend>>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>(), Arg.Any<string>())
                .Returns(restResponse);

            string collectionName = "collection";

            var client = new Client(null, null, request: request);
            IList<Friend> friends = client.GetEntities<Friend>(collectionName);

            Assert.IsNotNull(friends);
            Assert.AreEqual(entities.Count, friends.Count);
            Assert.AreEqual(friend1.Name, friends[0].Name);
            Assert.AreEqual(friend1.Age, friends[0].Age);
            Assert.AreEqual(friend2.Name, friends[1].Name);
            Assert.AreEqual(friend2.Age, friends[1].Age);
        }
    }
}