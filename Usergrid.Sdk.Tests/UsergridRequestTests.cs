using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using System.Linq;

namespace Usergrid.Sdk.Tests
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage RequestMessage { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestMessage = request;
            await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent("OK")};
        }
    }

    [TestFixture]
    public class UsergridRequestTests
    {
        [Test]
        public void ExecuteJsonRequestAddsAuthorizationHeader() {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler();
            var restClient = new HttpClient(fakeHttpMessageHandler);
            var usergridRequest = new UsergridRequest("http://usergrid.com", "org", "app", restClient);
            usergridRequest.AccessToken = "accessToken";

            usergridRequest.ExecuteJsonRequest("/resource", HttpMethod.Post, new object());

            var message = fakeHttpMessageHandler.RequestMessage;
            Assert.IsTrue(message.Method == HttpMethod.Post && message.Headers.Any(h => h.Key == "Authorization" && h.Value.First() == "Bearer accessToken"));
        }

        [Test]
        public async void ExecuteJsonRequestAddsBodyAsJson()
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler();
            var restClient = new HttpClient(fakeHttpMessageHandler);
            var body = new { data = "test" };
            var usergridRequest = new UsergridRequest("http://usergrid.com", "org", "app", restClient);

            usergridRequest.ExecuteJsonRequest("/resource", HttpMethod.Post, body);

            var message = fakeHttpMessageHandler.RequestMessage;
            var content = await message.Content.ReadAsStringAsync();
            Assert.IsTrue(content == "{\"data\":\"test\"}");
            Assert.IsTrue(message.Method == HttpMethod.Post && message.Content.Headers.Any(h => h.Key == "Content-Type" && h.Value.First() == "application/json"));
        }
    }
}