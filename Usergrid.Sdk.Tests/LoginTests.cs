using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class LoginTests
    {
        [Test]
        public void ShouldLoginAndSetTheAccessToken()
        {
            const string accessToken = "access_token";

            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(new LoginResponse {AccessToken = accessToken});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);

            //Assert.That(client.AccessToken, Is.EqualTo(accessToken));
        }

        [Test]
        public void ShouldLoginWithApplicationCredentialsWithCorrectRequestBody()
        {
            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(new LoginResponse {AccessToken = "access_token"});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            const string clientLoginId = "client_login_id";
            const string clientSecret = "client_secret";

            var client = new Client(null, null, request: request);
            client.Login(clientLoginId, clientSecret, AuthType.ClientId);

            request
                .Received(1)
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Is<ClientIdLoginPayload>(d => d.GrantType == "client_credentials" && d.ClientId == clientLoginId && d.ClientSecret == clientSecret));
        }

        [Test]
        public void ShouldLoginWithUserCredentialsWithCorrectRequestBody()
        {
            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(new LoginResponse {AccessToken = "access_token"});

            const string clientLoginId = "client_login_id";
            const string clientSecret = "client_secret";

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Any<UserLoginPayload>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.Login(clientLoginId, clientSecret, AuthType.User);

            request
                .Received(1)
                .ExecuteJsonRequest<LoginResponse>(
                    Arg.Any<string>(),
                    Arg.Any<Method>(),
                    Arg.Is<UserLoginPayload>(d => d.GrantType == "password" && d.UserName == clientLoginId && d.Password == clientSecret));
        }

        [Test]
        public void ShouldNotPassAccessTokenWhenLoggingIn()
        {
            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(new LoginResponse {AccessToken = "access_token"});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);

            request.Received(1).ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>());
        }

        [Test]
        public void ShouldPostToCorrectEndPoint()
        {
            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(new LoginResponse {AccessToken = "access_token"});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            client.Login(null, null, AuthType.ClientId);

            request.Received(1).ExecuteJsonRequest<LoginResponse>(Arg.Is("/token"), Arg.Is(Method.POST), Arg.Any<object>());
        }

        [Test]
        public void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            string restResponseContent = new UsergridError {Description = "Invalid username or password", Error = "invalid_grant"}.Serialize();

            var restResponse = Substitute.For<IRestResponse<LoginResponse>>();
            restResponse.StatusCode.Returns(HttpStatusCode.BadRequest);
            restResponse.Content.Returns(restResponseContent);

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var client = new Client(null, null, request: request);
            try
            {
                client.Login(null, null, AuthType.ClientId);
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("invalid_grant", e.ErrorCode);
                Assert.AreEqual("Invalid username or password", e.Message);
            }
        }
    }
}