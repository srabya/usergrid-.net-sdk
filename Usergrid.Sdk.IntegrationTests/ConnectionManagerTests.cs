using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
	public class Customer
	{
		public string Name { get; set;}
		public string No { get; set;}
	}

	public class Order
	{
		public string Name {get; set;}
		public string Id { get;set;}
	}

	public class Address
	{
		public string Name {get;set;}
		public string AddressLine {get;set;}
	}


	[TestFixture]
	public class ConnectionManagerTests : BaseTest
	{
		[Test]
		public void ShouldCreateConnection()
		{
			var client = new Client(Organization, Application);
			client.Login(ClientId, ClientSecret, AuthType.ClientId);

			var customer = client.GetEntity<Customer> ("customers", "customer1");

			if (customer != null)
			{
				client.DeleteEntity ("customers", "customer1");
			} 

			var c = new Customer { Name = "customer1", No = "1" };
			client.CreateEntity ("customers", c);

			var order = client.GetEntity<Order> ("orders", "order1");

			if (order != null)
			{
				client.DeleteEntity ("orders", "order1");
			}

			var o = new Order { Name = "order1", Id = "1" };
			client.CreateEntity ("orders", o);

			customer = client.GetEntity<Customer> ("customers", "customer1");
			order = client.GetEntity<Order> ("orders", "order1");

			var connections = client.GetConnections (customer, "has");
			Assert.AreEqual (0, connections.Count);

			client.CreateConnection (customer, order, "has");

			connections = client.GetConnections (customer, "has");
			Assert.AreEqual (1, connections.Count);

			client.DeleteConnection (customer, order, "has");
			connections = client.GetConnections (customer, "has");
			Assert.AreEqual (0, connections.Count);

			client.DeleteEntity ("customers", "customer1");
			client.DeleteEntity ("orders", "order1");
		}
	}
}

