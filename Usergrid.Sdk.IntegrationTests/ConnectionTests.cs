using System.Collections.Generic;
using NUnit.Framework;
using Usergrid.Sdk.Model;
using System.Linq;

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
        public void ShouldCreateAndGetSimpleConnection() {
            IClient client = InitializeClientAndLogin(AuthType.Organization);
            DeleteEntityIfExists<Customer>(client, "customers", "customer1");
            DeleteEntityIfExists<Order>(client, "orders", "order1");
            DeleteDeviceIfExists(client, "device1");

            //create a customer, order and a device
            //then we will connect the order entity and device entity to the customer using the same connection name (has)
            //order and device are different types of entities
            client.CreateEntity("customers", new Customer
                {
                    Name = "customer1",
                    No = "1"
                });
            client.CreateEntity("orders", new Order
                {
                    Name = "order1",
                    Id = "1"
                });
            client.CreateDevice(
                new DeviceTests.MyCustomUserGridDevice()
                    {
                        Name = "device1",
                        DeviceType = "device type"
                    });

            //create a connection between customer1 and order1
            client.CreateConnection(new Connection()
            {
                ConnectorCollectionName = "customers",
                ConnectorIdentifier = "customer1",
                ConnecteeCollectionName = "orders",
                ConnecteeIdentifier = "order1",
                ConnectionName = "has"
            });
            //create a connection between customer1 and device1
            client.CreateConnection(new Connection()
            {
                ConnectorCollectionName = "customers",
                ConnectorIdentifier = "customer1",
                ConnecteeCollectionName = "devices",
                ConnecteeIdentifier = "device1",
                ConnectionName = "has"
            });

            //get the connections, supply only the connector details
            //we get a list of Usergrid entites
            IList<UsergridEntity> allConnections = client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = "customers", 
                    ConnectorIdentifier = "customer1", 
                    ConnectionName = "has"
                });
            Assert.AreEqual(2, allConnections.Count);
            Assert.True(allConnections.Any(c=>c.Name == "order1"));
            Assert.True(allConnections.Any(c=>c.Name == "device1"));

            //now, just get the devices for customer from the connection
            //we need to supply the connectee collection name
            IList<DeviceTests.MyCustomUserGridDevice> deviceConnections = client.GetConnections<DeviceTests.MyCustomUserGridDevice>(new Connection()
                {
                    ConnectorCollectionName = "customers", 
                    ConnectorIdentifier = "customer1", 
                    ConnecteeCollectionName = "devices", 
                    ConnectionName = "has"
                });
            Assert.AreEqual(1, deviceConnections.Count);
            Assert.AreEqual("device1", deviceConnections[0].Name);

            //now, just get the orders for customer from the connection
            //we need to supply the connectee collection name
            IList<Order> orderConnections = client.GetConnections<Order>(new Connection()
                {
                    ConnectorCollectionName = "customers", 
                    ConnectorIdentifier = "customer1", 
                    ConnecteeCollectionName = "orders", 
                    ConnectionName = "has"
                });
            Assert.AreEqual(1, orderConnections.Count);
            Assert.AreEqual("order1", orderConnections[0].Name);

            //delete the connections
            client.DeleteConnection(new Connection()
            {
                ConnectorCollectionName = "customers",
                ConnectorIdentifier = "customer1",
                ConnecteeCollectionName = "devices",
                ConnecteeIdentifier = "device1",
                ConnectionName = "has"
            });
            //delete the connections
            client.DeleteConnection(new Connection()
            {
                ConnectorCollectionName = "customers",
                ConnectorIdentifier = "customer1",
                ConnecteeCollectionName = "orders",
                ConnecteeIdentifier = "order1",
                ConnectionName = "has"
            });

            //verify that it is deleted
            var noConnections = client.GetConnections(new Connection()
                {
                    ConnectorCollectionName = "customers", 
                    ConnectorIdentifier = "customer1", 
                    ConnectionName = "has"
                });
            Assert.AreEqual(0, noConnections.Count);
        }

        [Test]
        public void ShouldGetAndGetSimpleConnection() {
            IClient client = InitializeClientAndLogin(AuthType.Organization);
            DeleteEntityIfExists<Customer>(client, "customers", "customer1");
            DeleteEntityIfExists<Order>(client, "orders", "order1");

            client.CreateEntity("customers", new Customer
                {
                    Name = "customer1",
                    No = "1"
                });
            client.CreateEntity("orders", new Order
                {
                    Name = "order1",
                    Id = "1"
                });

            //to get a collection you need connector details and the connection name
            var getConnectionDetails = new Connection()
                {
                    ConnectorCollectionName = "customers", 
                    ConnectorIdentifier = "customer1", 
                    ConnectionName = "has"
                };
            //to create/delete a collection you need all the connector and connectee details and the connection name.
            var createDeleteConnectionDetails = new Connection()
            {
                ConnectorCollectionName = "customers",
                ConnectorIdentifier = "customer1",
                ConnecteeCollectionName = "orders",
                ConnecteeIdentifier = "order1",
                ConnectionName = "has"
            };

            //no connections yet
            IList<UsergridEntity> connections = client.GetConnections(getConnectionDetails);
            Assert.AreEqual(0, connections.Count);

            //create a connection between customer1 and order1
            client.CreateConnection(createDeleteConnectionDetails);

            //verify the connection
            connections = client.GetConnections(getConnectionDetails);
            Assert.AreEqual(1, connections.Count);
            Assert.AreEqual("order1", connections[0].Name);

            //delete the connection
            client.DeleteConnection(createDeleteConnectionDetails);

            //verify that it is deleted
            connections = client.GetConnections(getConnectionDetails);
            Assert.AreEqual(0, connections.Count);
        }

        [Test]
        public void ShouldFollowUserViaConnections()
        {
            IClient client = InitializeClientAndLogin(AuthType.Organization);
            DeleteUserIfExists(client, "user1");
            DeleteUserIfExists(client, "user2");
            
            UsergridUser user1 = new UsergridUser { UserName = "user1", Email = "user1@gmail.com" };
            client.CreateUser(user1);
            UsergridUser user2 = new UsergridUser { UserName = "user2", Email = "user2@gmail.com" };
            client.CreateUser(user2);
            
            var conn = new Connection();
            conn.ConnectorIdentifier = "user1";
            conn.ConnectorCollectionName = "users";
            conn.ConnecteeIdentifier = "user2";
            conn.ConnecteeCollectionName = "users";
            conn.ConnectionName = "following";
            
            client.CreateConnection(conn);

            //verify the connections
            var user1Uuid = client.GetEntities<UsergridUser>("users", limit: 1, query: "select * where username='user1'").First().Uuid;
            var user2Uuid = client.GetEntities<UsergridUser>("users", limit: 1, query: "select * where username='user2'").First().Uuid;

            //user1 should be following user2
            IList<UsergridEntity> followingConnections = client.GetConnections(new Connection()
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "user1",
                ConnectionName = "following"
            });

            Assert.AreEqual(1, followingConnections.Count);
            Assert.AreEqual(user2Uuid, followingConnections[0].Uuid);

            //also, user1 should be in the followers connection of user2
            IList<UsergridEntity> followersConnections = client.GetConnections(new Connection()
            {
                ConnectorCollectionName = "users",
                ConnectorIdentifier = "user2",
                ConnectionName = "followers"
            });

            Assert.AreEqual(1, followersConnections.Count);
            Assert.AreEqual(user1Uuid, followersConnections[0].Uuid);
        }
    }
}