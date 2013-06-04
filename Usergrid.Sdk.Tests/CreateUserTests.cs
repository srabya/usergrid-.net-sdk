using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class CreateUserTests
    {
        [Test]
        public void ShouldPassCorrectAccessToken()
        {
            const string accessToken = "access_token";
            IUsergridRequest request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);
            var createUserResponse = Substitute.For<IRestResponse>();
            createUserResponse.StatusCode.Returns(HttpStatusCode.OK);

            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(createUserResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);

            client.CreateUser(new UsergridUser {UserName = "username"});

            request.Received(1).Execute(
                Arg.Is("/users"),
                Arg.Is(Method.POST),
                Arg.Is<UsergridUser>(u => (u.UserName.Equals("username")))
                );
        }

        [Test]
        public void ShouldPostToCorrectEndPoint()
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);

            var request = Substitute.For<IUsergridRequest>();
            request
                .Execute(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.CreateUser(new UsergridUser {UserName = "username"});

            request.Received(1).Execute(
                Arg.Is("/users"),
                Arg.Is(Method.POST),
                Arg.Is<UsergridUser>(u => (u.UserName.Equals("username"))));
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
                client.CreateUser(new UsergridUser {UserName = "username"});
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("unauthorized", e.ErrorCode);
                Assert.AreEqual("Subject does not have permission [applications:get:7aa6ad30-c070-11e2-a082-d54b82588eab:/users", e.Message);
            }
        }
    }
}