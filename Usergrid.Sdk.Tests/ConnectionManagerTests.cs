using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class ConnectionManagerTests
    {
        [SetUp]
        public void Setup()
        {
            _request = Substitute.For<IUsergridRequest>();
            _connectionManager = new ConnectionManager(_request);
        }

        private IUsergridRequest _request;
        private ConnectionManager _connectionManager;

        [Test]
        public void CreateConnectionShouldPostToCorrectEndpoint() {
            var connection = new Connection
                {
                    ConnectorCollectionName = "users",
                    ConnectorIdentifier = "userName",
                    ConnecteeCollectionName = "devices",
                    ConnecteeIdentifier = "deviceName",
                    ConnectionName = "has"
                };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            _connectionManager.CreateConnection(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest(
                    "/users/userName/has/devices/deviceName",
                    HttpMethod.Post);
        }

        [Test]
        public void CreateConnectionShouldThrowUsergridExceptionWhenBadResponse() {
            var connection = new Connection();
            var restResponseContent = new UsergridError {Description = "Exception message", Error = "error code"};
            IRestResponse restResponseWithBadRequest = Helpers.SetUpRestResponseWithContent(HttpStatusCode.BadRequest, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponseWithBadRequest));

            try
            {
                _connectionManager.CreateConnection(connection);
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("error code", e.ErrorCode);
                Assert.AreEqual("Exception message", e.Message);
            }
        }

        [Test]
        public async void GetConnectionsReturnsConnectionsAsList()
        {
            var connection = new Connection
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "userName",
                ConnectionName = "has"
            };

            var expectedEntities = new List<UsergridEntity>();
            var responseData = new UsergridGetResponse<UsergridEntity>() {Entities = expectedEntities};
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, responseData);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var returnedEntities = await _connectionManager.GetConnections(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest("/users/userName/has",HttpMethod.Get);
            Assert.AreEqual(expectedEntities, returnedEntities);
        }

        [Test]
        public async void GetConnectionsReturnsNullWhenConnectionIsNotFound()
        {
            var connection = new Connection
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "userName",
                ConnectionName = "has"
            };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.NotFound);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var returnedEntities = await _connectionManager.GetConnections(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest("/users/userName/has",HttpMethod.Get);

            Assert.IsNull(returnedEntities);
        }

        [Test]
        public async void GetConnectionsOfSpecificTypeReturnsConnectionsAsListOfConnecteeType()
        {
            var connection = new Connection
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "userName",
                ConnecteeCollectionName = "devices",
                ConnectionName = "has"
            };
            var expectedEntities = new List<UsergridDevice>();
            var responseData = new UsergridGetResponse<UsergridDevice>() { Entities = expectedEntities };
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, responseData);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var returnedEntities =await _connectionManager.GetConnections<UsergridDevice>(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest("/users/userName/has/devices", HttpMethod.Get);
            Assert.AreEqual(expectedEntities, returnedEntities);
        }

        [Test]
        public async void GetConnectionsOfSpecificTypeReturnsNullWhenConnectionIsNotFound()
        {
            var connection = new Connection
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "userName",
                ConnecteeCollectionName = "devices",
                ConnectionName = "has"
            };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.NotFound);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var returnedEntities =await _connectionManager.GetConnections<UsergridDevice>(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest("/users/userName/has/devices",HttpMethod.Get);

            Assert.IsNull(returnedEntities);
        }


        [Test]
        public void DeleteConnectionShouldDeleteToCorrectEndpoint()
        {
            var connection = new Connection
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "userName",
                ConnecteeCollectionName = "devices",
                ConnecteeIdentifier = "deviceName",
                ConnectionName = "has"
            };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            _connectionManager.DeleteConnection(connection);

            _request
                .Received(1)
                .ExecuteJsonRequest(
                    "/users/userName/has/devices/deviceName",
                    HttpMethod.Delete);
        }

        [Test]
        public void DeleteConnectionShouldThrowUsergridExceptionWhenBadResponse()
        {
            var connection = new Connection();
            var restResponseContent = new UsergridError { Description = "Exception message", Error = "error code" };
            IRestResponse restResponseWithBadRequest = Helpers.SetUpRestResponseWithContent(HttpStatusCode.BadRequest, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponseWithBadRequest));

            try
            {
                _connectionManager.DeleteConnection(connection);
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("error code", e.ErrorCode);
                Assert.AreEqual("Exception message", e.Message);
            }
        }

    }
}