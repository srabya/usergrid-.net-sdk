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
    public class LoginTests
    {
        [Test]
        public void ShouldPostToCorrectEndPoint()
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(new LoginResponse { AccessToken = "access_token" }.Serialize());

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.Organization);

            request.Received(1).ExecuteJsonRequest(Arg.Is("/token"), Arg.Is(HttpMethod.Post), Arg.Any<object>());
        }
        
        [Test]
        public void ShouldLoginWithApplicationCredentialsWithCorrectRequestBody()
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(new LoginResponse { AccessToken = "access_token" }.Serialize());

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string clientLoginId = "client_login_id";
            const string clientSecret = "client_secret";

            var client = new Client(null, null, request: request);
            client.Login(clientLoginId, clientSecret, AuthType.Organization);

            request
                .Received(1)
                .ExecuteJsonRequest(
                    Arg.Any<string>(),
                    Arg.Any<HttpMethod>(),
                    Arg.Is<ClientIdLoginPayload>(d => d.GrantType == "client_credentials" && d.ClientId == clientLoginId && d.ClientSecret == clientSecret));
        }

        [Test]
        public void ShouldLoginWithUserCredentialsWithCorrectRequestBody()
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(new LoginResponse { AccessToken = "access_token" }.Serialize());

            const string clientLoginId = "client_login_id";
            const string clientSecret = "client_secret";

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(
                    Arg.Any<string>(),
                    Arg.Any<HttpMethod>(),
                    Arg.Any<UserLoginPayload>())
                .Returns(Task.FromResult(restResponse));

            var client = new Client(null, null, request: request);
            client.Login(clientLoginId, clientSecret, AuthType.User);

            request
                .Received(1)
                .ExecuteJsonRequest(
                    Arg.Any<string>(),
                    Arg.Any<HttpMethod>(),
                    Arg.Is<UserLoginPayload>(d => d.GrantType == "password" && d.UserName == clientLoginId && d.Password == clientSecret));
        }

        [Test]
        public async void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            string restResponseContent = new UsergridError { Description = "Invalid username or password", Error = "invalid_grant" }.Serialize();

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.BadRequest);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var client = new Client(null, null, request: request);
            try
            {
                await client.Login(null, null, AuthType.Organization);
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("invalid_grant", e.ErrorCode);
                Assert.AreEqual("Invalid username or password", e.Message);
            }
        }

        [Test]
        public void ShouldLoginAndSetTheAccessToken()
        {
            const string accessToken = "access_token";

            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(new LoginResponse { AccessToken = accessToken }.Serialize());

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.Organization);

            Assert.That(request.AccessToken, Is.EqualTo(accessToken));
        }
    }
}