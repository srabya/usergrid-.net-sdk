using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    public class Customer {
        public string Name { get; set; }
        public string No { get; set; }
    }

    public class Order {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    [TestFixture]
    public class ConnectionTests : BaseTest {
        [Test]
        public async void ShouldCreateAndGetSimpleConnection() {
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            string customersCollectionName = "newcustomers";
            string ordersCollectionName = "neworders";
            string deviceName = "device2";

            await DeleteEntityIfExists<Customer>(client, customersCollectionName, "customer1");
            await DeleteEntityIfExists<Order>(client, ordersCollectionName, "order1");
            await DeleteDeviceIfExists(client, deviceName);

            //create a customer, order and a device
            //then we will connect the order entity and device entity to the customer using the same connection name (has)
            //order and device are different types of entities
            await client.CreateEntity(customersCollectionName, new Customer
                {
                    Name = "customer1",
                    No = "1"
                });
            await client.CreateEntity(ordersCollectionName, new Order
                {
                    Name = "order1",
                    Id = "1"
                });
            await client.CreateDevice(
                new DeviceTests.MyCustomUserGridDevice
                    {
                        Name = deviceName,
                        DeviceType = "device type"
                    });

            //create a connection between customer1 and order1
            await client.CreateConnection(new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = ordersCollectionName,
                    ConnecteeIdentifier = "order1",
                    ConnectionName = "has"
                });
            //create a connection between customer1 and device1
            await client.CreateConnection(new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = "devices",
                    ConnecteeIdentifier = deviceName,
                    ConnectionName = "has"
                });

            //get the connections, supply only the connector details
            //we get a list of Usergrid entites
            IList<UsergridEntity> allConnections = await client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnectionName = "has"
                });
            Assert.AreEqual(2, allConnections.Count);
            Assert.True(allConnections.Any(c => c.Name == "order1"));
            Assert.True(allConnections.Any(c => c.Name == deviceName));

            //now, just get the devices for customer from the connection
            //we need to supply the connectee collection name
            IList<DeviceTests.MyCustomUserGridDevice> deviceConnections = await client.GetConnections<DeviceTests.MyCustomUserGridDevice>(new Connection()
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = "devices",
                    ConnectionName = "has"
                });
            Assert.AreEqual(1, deviceConnections.Count);
            Assert.AreEqual(deviceName, deviceConnections[0].Name);

            //now, just get the orders for customer from the connection
            //we need to supply the connectee collection name
            IList<Order> orderConnections = await client.GetConnections<Order>(new Connection()
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = ordersCollectionName,
                    ConnectionName = "has"
                });
            Assert.AreEqual(1, orderConnections.Count);
            Assert.AreEqual("order1", orderConnections[0].Name);

            //delete the connections
            await client.DeleteConnection(new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = "devices",
                    ConnecteeIdentifier = deviceName,
                    ConnectionName = "has"
                });
            //delete the connections
            await client.DeleteConnection(new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = ordersCollectionName,
                    ConnecteeIdentifier = "order1",
                    ConnectionName = "has"
                });

            //verify that it is deleted
            var noConnections = await client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnectionName = "has"
                });
            Assert.AreEqual(0, noConnections.Count);
        }

        [Test]
        public async void ShouldFollowUserViaConnections() {
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            string user1Identifier = "newuser11";
            string user2Identifier = "newuser22";

            var user1 = await SetupUsergridUser(client, new UsergridUser { UserName = user1Identifier, Email = "newuser11@gmail.com" });
            var user2 = await SetupUsergridUser(client, new UsergridUser { UserName = user2Identifier, Email = "newuser22@gmail.com" });

            var conn = new Connection();
            conn.ConnectorIdentifier = user1Identifier;
            conn.ConnectorCollectionName = "users";
            conn.ConnecteeIdentifier = user2Identifier;
            conn.ConnecteeCollectionName = "users";
            conn.ConnectionName = "following";

            await client.CreateConnection(conn);

            //user1 should be following user2
            IList<UsergridEntity> followingConnections = await client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = "users",
                    ConnectorIdentifier = user1Identifier,
                    ConnectionName = "following"
                });

            Assert.AreEqual(1, followingConnections.Count);
            Assert.AreEqual(user2.Uuid, followingConnections[0].Uuid);

            //also, user1 should be in the followers connection of user2
            IList<UsergridEntity> followersConnections = await client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = "users",
                    ConnectorIdentifier = user2Identifier,
                    ConnectionName = "followers"
                });

            Assert.AreEqual(1, followersConnections.Count);
            Assert.AreEqual(user1.Uuid, followersConnections[0].Uuid);
        }

        [Test]
        public async void ShouldGetAndGetSimpleConnection() {
            IClient client = await InitializeClientAndLogin(AuthType.Organization);
            string customersCollectionName = "newcustomers";
            string ordersCollectionName = "neworders";

            await DeleteEntityIfExists<Customer>(client, customersCollectionName, "customer1");
            await DeleteEntityIfExists<Order>(client, ordersCollectionName, "order1");

            await client.CreateEntity(customersCollectionName, new Customer
                {
                    Name = "customer1",
                    No = "1"
                });
            await client.CreateEntity(ordersCollectionName, new Order
                {
                    Name = "order1",
                    Id = "1"
                });

            //to get a collection you need connector details and the connection name
            var getConnectionDetails = new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnectionName = "has"
                };
            //to create/delete a collection you need all the connector and connectee details and the connection name.
            var createDeleteConnectionDetails = new Connection
                {
                    ConnectorCollectionName = customersCollectionName,
                    ConnectorIdentifier = "customer1",
                    ConnecteeCollectionName = ordersCollectionName,
                    ConnecteeIdentifier = "order1",
                    ConnectionName = "has"
                };

            //no connections yet
            IList<UsergridEntity> connections = await client.GetConnections(getConnectionDetails);
            Assert.AreEqual(0, connections.Count);

            //create a connection between customer1 and order1
            await client.CreateConnection(createDeleteConnectionDetails);

            //verify the connection
            connections = await client.GetConnections(getConnectionDetails);
            Assert.AreEqual(1, connections.Count);
            Assert.AreEqual("order1", connections[0].Name);

            //delete the connection
            await client.DeleteConnection(createDeleteConnectionDetails);

            //verify that it is deleted
            connections = await client.GetConnections(getConnectionDetails);
            Assert.AreEqual(0, connections.Count);
        }
    }
}