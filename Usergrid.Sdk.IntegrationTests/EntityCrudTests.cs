using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
	[TestFixture]
	public class EntityCrudTests : BaseTest
	{
		[Test]
		public void ShouldCrud()
		{
			var entityName = "Name-" + GetRandomInteger (1, 100);
			var age = GetRandomInteger (1, 50);
			var friend = new Friend {
				Name = entityName,
				Age = age
			};

			var client = new Client (Organization, Application);
			client.Login(ClientId, ClientSecret, AuthType.ClientId);

			// try to fetch the entity
			var friendFromUsergrid = client.GetEntity<Friend> ("friends", entityName);

			// Delete the entity if it already exists
			if (friendFromUsergrid != null)
			{
				client.DeleteEntity ("friends", entityName);
			}

            // Create a new entity
			client.CreateEntity ("friends", friend);

			// Get it back
			friendFromUsergrid = client.GetEntity<Friend> ("friends", entityName);

			// Assert that the entity returned is correct
			Assert.IsNotNull (friendFromUsergrid);
			Assert.IsNotNull (friendFromUsergrid.Uuid);
			Assert.AreEqual (entityName, friendFromUsergrid.Name);
			Assert.AreEqual (entityName, friendFromUsergrid.Entity.Name);
			Assert.AreEqual (age, friendFromUsergrid.Entity.Age);

			// Get it back with query
			var query = "select * where name = '" + entityName + "'";
			var friends = client.GetEntities<Friend> ("friends", query: query);

			// Assert the collection is correct
			Assert.IsNotNull (friends);
			Assert.AreEqual (1, friends.Count);
			Assert.IsFalse (friends.HasNext);
			Assert.IsFalse (friends.HasPrevious);
			friendFromUsergrid = friends [0];
			Assert.IsNotNull (friendFromUsergrid);
			Assert.IsNotNull (friendFromUsergrid.Uuid);
			Assert.AreEqual (entityName, friendFromUsergrid.Name);
			Assert.AreEqual (entityName, friendFromUsergrid.Entity.Name);
			Assert.AreEqual (age, friendFromUsergrid.Entity.Age);

			friend = friendFromUsergrid.Entity;

			// Update the entity
			var newAge = GetRandomInteger (1, 50);
			friend.Age = newAge;
			client.UpdateEntity ("friends", entityName, friend);

			// Get it back
			friendFromUsergrid = client.GetEntity<Friend> ("friends", entityName);

			// Assert that entity is updated
			Assert.AreEqual (newAge, friendFromUsergrid.Entity.Age);

			// Delete the entity
			client.DeleteEntity ("friends", entityName);

			// Get it back
			friendFromUsergrid = client.GetEntity<Friend> ("friends", entityName);

			// Assert that it doesn't exist
			Assert.IsNull (friendFromUsergrid);

		}
	}
}

