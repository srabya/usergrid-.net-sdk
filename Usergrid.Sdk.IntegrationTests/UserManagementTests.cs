using NUnit.Framework;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
    public class MyUsergridUser : UsergridUser
    {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    [TestFixture]
    public class UserManagementTests : BaseTest
    {
        [Test]
        public void ShouldChangePassword()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);

            var user = client.GetUser<MyUsergridUser>("user1");

            if (user != null)
                client.DeleteUser("user1");

            user = new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
            client.CreateUser(user);

            user = client.GetUser<MyUsergridUser>("user1");
            Assert.IsNotNull(user);

            client.Login("user1", "user1", AuthType.User);

            client.ChangePassword("user1", "user1", "user1-2");

            client.Login("user1", "user1-2", AuthType.User);
        }

        [Test]
        public void ShouldCreateUser()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);

            var user = client.GetUser<MyUsergridUser>("user1");

            if (user != null)
            {
                client.DeleteUser("user1");
            }

            user = new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
            client.CreateUser(user);
            user = client.GetUser<MyUsergridUser>("user1");

            Assert.IsNotNull(user);
            Assert.AreEqual("user1", user.UserName);
            Assert.AreEqual("user1@gmail.com", user.Email);
            Assert.AreEqual("city1", user.City);
            Assert.IsNull(user.Password);

            client.DeleteUser("user1");
        }

        [Test]
        public void ShouldUpdateUser()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);

            var user = client.GetUser<MyUsergridUser>("user1");
            if (user == null)
            {
                user = new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
                client.CreateUser(user);
                user = client.GetUser<MyUsergridUser>("user1");
            }
            else
            {
                client.DeleteUser("user1");
            }

            user.Email = "user-2@gmail.com";
            user.City = "city1-2";
            user.Password = "user1-2";
            client.UpdateUser(user);

            user = client.GetUser<MyUsergridUser>("user1");
            Assert.IsNotNull(user);
            Assert.AreEqual("user1", user.UserName);
            Assert.AreEqual("user-2@gmail.com", user.Email);
            Assert.AreEqual("city1-2", user.City);
            Assert.IsNull(user.Password);

            client.DeleteUser("user1");
        }
    }
}