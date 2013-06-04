using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class CreateEntityTests
    {
        [Test]
        public void ShouldPassCorrectAccessToken()
        {
            const string accessToken = "access_token";
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);
            var createEntityResponse = Substitute.For<IRestResponse>();
            createEntityResponse.StatusCode.Returns(HttpStatusCode.OK);

            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(createEntityResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);

            const string collectionName = "collection";
            var entityToPost = new {FirstName = "first", LastName = "last"};

            client.CreateEntity(collectionName, entityToPost);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}", collectionName)),
                Arg.Is(Method.POST),
                Arg.Is(entityToPost));
        }

        [Test]
        public void ShouldPostToCorrectEndPoint()
        {
            const string collectionName = "collection";
            var entityToPost = new {FirstName = "first", LastName = "last"};

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.CreateEntity(collectionName, entityToPost);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}", collectionName)),
                Arg.Is(Method.POST),
                Arg.Is(entityToPost));
        }

        [Test]
        public void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            string restResponseContent = new UsergridError
                {
                    Description = "Subject does not have permission [applications:get:7aa6ad30-c070-11e2-a082-d54b82588eab:/users",
                    Error = "unauthorized"
                }
                .Serialize();

            const string collectionName = "collection";
            var entityToPost = new {FirstName = "first", LastName = "last"};

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.BadRequest);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            try
            {
                client.CreateEntity(collectionName, entityToPost);
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("unauthorized", e.ErrorCode);
                Assert.AreEqual("Subject does not have permission [applications:get:7aa6ad30-c070-11e2-a082-d54b82588eab:/users", e.Message);
            }
        }
    }
}