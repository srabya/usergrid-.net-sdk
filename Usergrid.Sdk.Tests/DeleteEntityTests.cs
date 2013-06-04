using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class DeleteEntityTests
    {
        [Test]
        public void ShouldPassCorrectAccessToken()
        {
            const string accessToken = "access_token";
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);
            const string collection = "collection";
            const string entity = "entity";

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);
            client.DeleteEntity(collection, entity);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}/{1}", collection, entity)),
                Arg.Is(Method.DELETE),
                Arg.Any<object>());
        }

        [Test]
        public void ShouldSendDeleteToCorrectEndPoint()
        {
            const string collection = "collection";
            const string entity = "entity";

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.DeleteEntity(collection, entity);

            request.Received(1).Execute(
                Arg.Is(string.Format("/{0}/{1}", collection, entity)),
                Arg.Is(Method.DELETE),
                Arg.Any<object>());
        }

        [Test]
        public void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            string restResponseContent = new UsergridError
                {
                    Description = "Service resource not found",
                    Error = "service_resource_not_found"
                }
                .Serialize();

            const string collection = "collection";
            const string entity = "entity";

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.NotFound);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            try
            {
                client.DeleteEntity(collection, entity);
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("service_resource_not_found", e.ErrorCode);
                Assert.AreEqual("Service resource not found", e.Message);
            }
        }
    }
}