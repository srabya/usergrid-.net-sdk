using System.Collections.Generic;
using NSubstitute;
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
            var connectorEntity = new UsergridEntity();
            var connecteeEntity = new UsergridEntity();

            _client.CreateConnection(connectorEntity, connecteeEntity, "connectionName");

            _connectionManager.Received(1).CreateConnection(connectorEntity, connecteeEntity, "connectionName");
        }

        [Test]
        public void DeleteConnectionsShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connectorEntity = new UsergridEntity();
            var connecteeEntity = new UsergridEntity();

            _client.DeleteConnection(connectorEntity, connecteeEntity, "connectionName");

            _connectionManager.Received(1).DeleteConnection(connectorEntity, connecteeEntity, "connectionName");
        }

        [Test]
        public void GetConnectionsOfSpecificTypeShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connectorEntity = new UsergridEntity();
            var enityList = new List<UsergridEntity<UsergridEntity>>();

            _connectionManager.GetConnections<UsergridEntity, UsergridEntity>(connectorEntity, "connectionName").Returns(enityList);

            IList<UsergridEntity<UsergridEntity>> returnedEntityList = _client.GetConnections<UsergridEntity, UsergridEntity>(connectorEntity, "connectionName");

            _connectionManager.Received(1).GetConnections<UsergridEntity, UsergridEntity>(connectorEntity, "connectionName");
            Assert.AreEqual(enityList, returnedEntityList);
        }

        [Test]
        public void GetConnectionsShouldDelegateToConnectionManagerWithCorrectParameters()
        {
            var connectorEntity = new UsergridEntity();
            var enityList = new List<UsergridEntity>();
            _connectionManager.GetConnections(connectorEntity, "connectionName").Returns(enityList);

            IList<UsergridEntity> returnedEntityList = _client.GetConnections(connectorEntity, "connectionName");

            _connectionManager.Received(1).GetConnections(connectorEntity, "connectionName");
            Assert.AreEqual(enityList, returnedEntityList);
        }
    }
}