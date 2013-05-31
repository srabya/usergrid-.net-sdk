using System.Collections.Generic;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
    public class MyGroup : Group
    {
        public string Description { get; set; }
    }

    [TestFixture]
    public class GroupTests : BaseTest
    {
        [Test]
        public void ShouldManageGroupLifecycle()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);

            var group = client.GetGroup<MyGroup>("group1");

            if (group != null)
                client.DeleteGroup("group1");

            group = new MyGroup {Path = "group1", Title = "title1", Description = "desc1"};
            client.CreateGroup(group);
            group = client.GetGroup<MyGroup>("group1");

            Assert.IsNotNull(group);
            Assert.AreEqual("group1", group.Path);
            Assert.AreEqual("title1", group.Title);
            Assert.AreEqual("desc1", group.Description);

            group.Description = "desc2";
            group.Title = "title2";

            client.UpdateGroup(group);

            group = client.GetGroup<MyGroup>("group1");

            Assert.IsNotNull(group);
            Assert.AreEqual("group1", group.Path);
            Assert.AreEqual("title2", group.Title);
            Assert.AreEqual("desc2", group.Description);

            client.DeleteGroup("group1");

            group = client.GetGroup<MyGroup>("group1");
            Assert.IsNull(group);
        }

        [Test]
        public void ShouldManageUsersInGroup()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);

            var user = client.GetUser<MyUser>("user1");

            if (user != null)
            {
                client.DeleteUser("user1");
            }

            user = new MyUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
            client.CreateUser(user);
            user = client.GetUser<MyUser>("user1");
            Assert.IsNotNull(user);

            var group = client.GetGroup<MyGroup>("group1");

            if (group != null)
                client.DeleteGroup("group1");

            group = new MyGroup {Path = "group1", Title = "title1", Description = "desc1"};
            client.CreateGroup(group);
            group = client.GetGroup<MyGroup>("group1");
            Assert.IsNotNull(group);

            client.AddUserToGroup(group.Path, user.UserName);
            IList<User> users = client.GetAllUsersInGroup<User>(group.Path);
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);

            client.DeleteUserFromGroup("group1", "user1");
            users = client.GetAllUsersInGroup<User>(group.Path);
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count);

            client.DeleteGroup("group1");
            client.DeleteUser("user1");
        }
    }
}