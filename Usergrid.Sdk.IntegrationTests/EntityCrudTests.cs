using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    [TestFixture]
    public class EntityCrudTests : BaseTest {
        private IClient _client;

        [Test]
        public async void ShouldCrudPocoEntity() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            const string collectionName = "newfriends";
            var friend = new Friend
                {
                    Name = "EntityName",
                    Age = 25
                };

            await DeleteEntityIfExists<Friend>(_client, collectionName, friend.Name);

            // Create a new entity
            await _client.CreateEntity(collectionName, friend);

            // Get it back
            var friendFromUsergrid = await _client.GetEntity<Friend>(collectionName, friend.Name);

            // Assert that the entity returned is correct
            Assert.IsNotNull(friendFromUsergrid);
            Assert.AreEqual(friend.Name, friendFromUsergrid.Name);
            Assert.AreEqual(friend.Age, friendFromUsergrid.Age);

            // Get it back with query
            string query = "select * where name = '" + friend.Name + "'";
            UsergridCollection<Friend> friends = await _client.GetEntities<Friend>(collectionName, query: query);

            // Assert the collection is correct
            Assert.IsNotNull(friends);
            Assert.AreEqual(1, friends.Count);
            Assert.IsFalse(friends.HasNext);
            Assert.IsFalse(friends.HasPrevious);
            friendFromUsergrid = friends[0];
            Assert.IsNotNull(friendFromUsergrid);
            Assert.AreEqual(friend.Name, friendFromUsergrid.Name);
            Assert.AreEqual(friend.Age, friendFromUsergrid.Age);


            // Update the entity
            Friend friendToUpdate = friendFromUsergrid;
            friendToUpdate.Age = 30;
            await _client.UpdateEntity(collectionName, friendToUpdate.Name, friendToUpdate);

            // Get it back
            friendFromUsergrid = await _client.GetEntity<Friend>(collectionName, friendToUpdate.Name);

            // Assert that entity is updated
            Assert.AreEqual(friendToUpdate.Age, friendFromUsergrid.Age);

            // Delete the entity
            await _client.DeleteEntity(collectionName, friend.Name);

            // Get it back
            friendFromUsergrid = await _client.GetEntity<Friend>(collectionName, friend.Name);

            // Assert that it doesn't exist
            Assert.IsNull(friendFromUsergrid);
        }

        [Test]
        public async void ShouldCrudUserGridEntity() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            const string collectionName = "newfriends";
            var friend = new UsergridFriend
                {
                    Name = "EntityName",
                    Age = 25
                };

            await DeleteEntityIfExists<Friend>(_client, collectionName, friend.Name);

            // Create a new entity
            await _client.CreateEntity(collectionName, friend);

            // Get it back
            var friendFromUsergrid = await _client.GetEntity<UsergridFriend>(collectionName, friend.Name);

            // Assert that the entity returned is correct
            Assert.IsNotNull(friendFromUsergrid);
            Assert.IsNotEmpty(friendFromUsergrid.Uuid);
            Assert.IsNotEmpty(friendFromUsergrid.Type);
            Assert.IsTrue(friendFromUsergrid.CreatedDate > DateTime.MinValue);
            Assert.IsTrue(friendFromUsergrid.ModifiedDate > DateTime.MinValue);
            Assert.AreEqual(friend.Name, friendFromUsergrid.Name);
            Assert.AreEqual(friend.Age, friendFromUsergrid.Age);

            // Get it back with query
            string query = "select * where name = '" + friend.Name + "'";
            UsergridCollection<UsergridFriend> friends = await _client.GetEntities<UsergridFriend>(collectionName, query: query);

            // Assert the collection is correct
            Assert.IsNotNull(friends);
            Assert.AreEqual(1, friends.Count);
            Assert.IsFalse(friends.HasNext);
            Assert.IsFalse(friends.HasPrevious);
            friendFromUsergrid = friends[0];
            Assert.IsNotNull(friendFromUsergrid);
            Assert.AreEqual(friend.Name, friendFromUsergrid.Name);
            Assert.AreEqual(friend.Age, friendFromUsergrid.Age);
            Assert.IsNotEmpty(friendFromUsergrid.Uuid);
            Assert.IsNotEmpty(friendFromUsergrid.Type);
            Assert.IsTrue(friendFromUsergrid.CreatedDate > DateTime.MinValue);
            Assert.IsTrue(friendFromUsergrid.ModifiedDate > DateTime.MinValue);


            // Update the entity
            UsergridFriend friendToUpdate = friendFromUsergrid;
            friendToUpdate.Age = 30;
            await _client.UpdateEntity(collectionName, friendToUpdate.Name, friendToUpdate);

            // Get it back
            friendFromUsergrid = await _client.GetEntity<UsergridFriend>(collectionName, friendToUpdate.Name);

            // Assert that entity is updated
            Assert.AreEqual(friendToUpdate.Age, friendFromUsergrid.Age);

            // Delete the entity
            await _client.DeleteEntity(collectionName, friend.Name);

            // Get it back
            friendFromUsergrid = await _client.GetEntity<UsergridFriend>(collectionName, friend.Name);

            // Assert that it doesn't exist
            Assert.IsNull(friendFromUsergrid);
        }
    }
}