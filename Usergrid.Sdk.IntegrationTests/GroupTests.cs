using System.Collections.Generic;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    public class MyUsergridGroup : UsergridGroup {
        public string Description { get; set; }
    }

    [TestFixture]
    public class GroupTests : BaseTest {
        [Test]
        public async void ShouldManageGroupLifecycle() {
            var client = new Client(Organization, Application);
            await client.Login(ClientId, ClientSecret, AuthType.Organization);

            var group = await client.GetGroup<MyUsergridGroup>("group1");

            if (group != null)
                await client.DeleteGroup("group1");

            group = new MyUsergridGroup {Path = "group1", Title = "title1", Description = "desc1"};
            await client.CreateGroup(group);
            group = await client.GetGroup<MyUsergridGroup>("group1");

            Assert.IsNotNull(group);
            Assert.AreEqual("group1", group.Path);
            Assert.AreEqual("title1", group.Title);
            Assert.AreEqual("desc1", group.Description);

            group.Description = "desc2";
            group.Title = "title2";

            await client.UpdateGroup(group);

            group = await client.GetGroup<MyUsergridGroup>("group1");

            Assert.IsNotNull(group);
            Assert.AreEqual("group1", group.Path);
            Assert.AreEqual("title2", group.Title);
            Assert.AreEqual("desc2", group.Description);

            await client.DeleteGroup("group1");

            group = await client.GetGroup<MyUsergridGroup>("group1");
            Assert.IsNull(group);
        }

        [Test]
        public async void ShouldManageUsersInGroup() {
            var client = new Client(Organization, Application);
            await client.Login(ClientId, ClientSecret, AuthType.Organization);

            UsergridUser user = await SetupUsergridUser(client, new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"});
            UsergridGroup group = await SetupUsergridGroup(client, new MyUsergridGroup {Path = "group1", Title = "title1", Description = "desc1"});

            await client.AddUserToGroup(group.Path, user.UserName);
            IList<UsergridUser> users = await client.GetAllUsersInGroup<UsergridUser>(group.Path);
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);

            await client.DeleteUserFromGroup("group1", "user1");
            users = await client.GetAllUsersInGroup<UsergridUser>(group.Path);
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count);

            await client.DeleteGroup("group1");
            await client.DeleteUser("user1");
        }
    }
}