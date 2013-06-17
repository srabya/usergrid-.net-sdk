using System.Collections.Generic;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
    public class Customer
    {
        public string Name { get; set; }
        public string No { get; set; }
    }

    public class Order
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }
        public string AddressLine { get; set; }
    }


    [TestFixture]
    public class ConnectionTests : BaseTest
    {
        [Test]
        public void ShouldCreateAndGetConnection()
        {
            IClient client = InitializeClientAndLogin(AuthType.ClientId);
            DeleteEntityIfExists<Customer>(client, "customers", "customer1");
            DeleteEntityIfExists<Order>(client, "orders", "order1");

            UsergridEntity<Customer> customer = client.CreateEntity("customers", new Customer
                                                                                     {
                                                                                         Name = "customer1",
                                                                                         No = "1"
                                                                                     });
            UsergridEntity<Order> order = client.CreateEntity("orders", new Order
                                                                            {
                                                                                Name = "order1",
                                                                                Id = "1"
                                                                            });

            IList<UsergridEntity> connections = client.GetConnections(customer, "has");
            Assert.AreEqual(0, connections.Count);

            client.CreateConnection(customer, order, "has");

            connections = client.GetConnections(customer, "has");
            Assert.AreEqual(1, connections.Count);
            Assert.AreEqual("order1", connections[0].Name);

            client.DeleteConnection(customer, order, "has");
            connections = client.GetConnections(customer, "has");
            Assert.AreEqual(0, connections.Count);
        }
    }
}