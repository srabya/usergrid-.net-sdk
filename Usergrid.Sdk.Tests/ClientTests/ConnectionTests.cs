using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests.ClientTests
{
    [TestFixture]
    public class ConnectionTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _connectionManager = Substitute.For<IConnectionManager>();
            _client = new Client(null, null) {ConnectionManager = _connectionManager};
        }

        #endregion

        private IConnectionManager _connectionManager;
        private IClient _client;

        [Test]
        public void CreateConnectionShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connection = new Connection();
            _client.CreateConnection(connection);

            _connectionManager.Received(1).CreateConnection(connection);
        }

        [Test]
        public void DeleteConnectionsShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connection = new Connection();

            _client.DeleteConnection(connection);

            _connectionManager.Received(1).DeleteConnection(connection);
        }

        [Test]
        public async void GetConnectionsOfSpecificTypeShouldDelegateToConnectionManagerWithCorrectParameters() {
            var connection = new Connection();
            IList<UsergridEntity> entityList = new List<UsergridEntity>();

            _connectionManager.GetConnections<UsergridEntity>(connection).Returns(delegate(CallInfo info) { return Task.FromResult(entityList); });
            Task<IList<UsergridEntity>>.FromResult(entityList);
   

            IList<UsergridEntity> returnedEntityList = await _client.GetConnections<UsergridEntity>(connection);

            _connectionManager.Received(1).GetConnections<UsergridEntity>(connection);
            Assert.AreEqual(entityList, returnedEntityList);
        }

        [Test]
        public async void GetConnectionsShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connection = new Connection();
            IList<UsergridEntity> entityList = new List<UsergridEntity>();
            _connectionManager.GetConnections(connection).Returns(delegate(CallInfo info) { return Task.FromResult(entityList); });

            IList<UsergridEntity> returnedEntityList = await _client.GetConnections(connection);

            _connectionManager.Received(1).GetConnections(connection);
            Assert.AreEqual(entityList, returnedEntityList);
        }
    }
}