using System.Collections.Generic;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
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
        public void CreateConnectionShouldPostToCorrectEndpoint()
        {
            var connectorEntity = new UsergridUser {Type = "user", Name = "userName"};
            var connecteeEntity = new UsergridDevice {Type = "device", Name = "deviceName"};
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            _connectionManager.CreateConnection(connectorEntity, connecteeEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest(
                    "/user/userName/has/device/deviceName",
                    Method.POST);
        }

        [Test]
        public void CreateConnectionShouldThrowUsergridExceptionWhenBadResponse()
        {
            var connectorEntity = new UsergridUser {Type = "user", Name = "userName"};
            var connecteeEntity = new UsergridDevice {Type = "device", Name = "deviceName"};
            var restResponseContent = new UsergridError {Description = "Exception message", Error = "error code"};
            IRestResponse<LoginResponse> restResponseWithBadRequest = Helpers.SetUpRestResponseWithContent<LoginResponse>(HttpStatusCode.BadRequest, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponseWithBadRequest);

            try
            {
                _connectionManager.CreateConnection(connectorEntity, connecteeEntity, "has");
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("error code", e.ErrorCode);
                Assert.AreEqual("Exception message", e.Message);
            }
        }

        [Test]
        public void GetConnectionsReturnsConnectionsAsList()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            var expectedEntities = new List<UsergridEntity>();
            var responseData = new UsergridGetResponse<UsergridEntity>() {Entities = expectedEntities};
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent<UsergridGetResponse<UsergridEntity>>(HttpStatusCode.OK, responseData);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var returnedEntities = _connectionManager.GetConnections(connectorEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest("/user/userName/has",Method.GET);
            Assert.AreEqual(expectedEntities, returnedEntities);
        }

        [Test]
        public void GetConnectionsReturnsNullWhenConnectionIsNotFound()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.NotFound);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var returnedEntities = _connectionManager.GetConnections(connectorEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest("/user/userName/has",Method.GET);

            Assert.IsNull(returnedEntities);
        }

        [Test]
        public void GetConnectionsOfSpecificTypeReturnsConnectionsAsListOfConnecteeType()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            var expectedEntities = new List<UsergridDevice>();
            var responseData = new UsergridGetResponse<UsergridDevice>() { Entities = expectedEntities };
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent<UsergridGetResponse<UsergridDevice>>(HttpStatusCode.OK, responseData);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var returnedEntities = _connectionManager.GetConnections<UsergridUser, UsergridDevice>(connectorEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest("/user/userName/has/" + typeof(UsergridDevice).Name, Method.GET);
            Assert.AreEqual(expectedEntities, returnedEntities);
        }

        [Test]
        public void GetConnectionsOfSpecificTypeReturnsNullWhenConnectionIsNotFound()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.NotFound);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            var returnedEntities = _connectionManager.GetConnections<UsergridUser, UsergridDevice>(connectorEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest("/user/userName/has/"+typeof(UsergridDevice).Name,Method.GET);

            Assert.IsNull(returnedEntities);
        }


        [Test]
        public void DeleteConnectionShouldDeleteToCorrectEndpoint()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            var connecteeEntity = new UsergridDevice { Type = "device", Name = "deviceName" };
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponse);

            _connectionManager.DeleteConnection(connectorEntity, connecteeEntity, "has");

            _request
                .Received(1)
                .ExecuteJsonRequest(
                    "/user/userName/has/device/deviceName",
                    Method.DELETE);
        }

        [Test]
        public void DeleteConnectionShouldThrowUsergridExceptionWhenBadResponse()
        {
            var connectorEntity = new UsergridUser { Type = "user", Name = "userName" };
            var connecteeEntity = new UsergridDevice { Type = "device", Name = "deviceName" };
            var restResponseContent = new UsergridError { Description = "Exception message", Error = "error code" };
            IRestResponse<LoginResponse> restResponseWithBadRequest = Helpers.SetUpRestResponseWithContent<LoginResponse>(HttpStatusCode.BadRequest, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(restResponseWithBadRequest);

            try
            {
                _connectionManager.DeleteConnection(connectorEntity, connecteeEntity, "has");
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