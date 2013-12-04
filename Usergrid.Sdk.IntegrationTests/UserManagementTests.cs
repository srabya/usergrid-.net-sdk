using NUnit.Framework;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    public class MyUsergridUser : UsergridUser {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    [TestFixture]
    public class UserManagementTests : BaseTest {
        private IClient _client;

        [Test]
        public async void ShouldChangePassword() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteUserIfExists(_client, "user1");

            var user = new MyUsergridUser {UserName = "user1", Password = "password1", Email = "user1@gmail.com", City = "city1"};
            await _client.CreateUser(user);

            user = await _client.GetUser<MyUsergridUser>("user1");
            Assert.IsNotNull(user);

            await _client.Login("user1", "password1", AuthType.User);

            await _client.ChangePassword("user1", "password1", "password2");

            await _client.Login("user1", "password2", AuthType.User);
        }

        [Test]
        public async void ShouldCreateUser() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteUserIfExists(_client, "user1");

            var user = new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
            await _client.CreateUser(user);
            user = await _client.GetUser<MyUsergridUser>("user1");

            Assert.IsNotNull(user);
            Assert.AreEqual("user1", user.UserName);
            Assert.AreEqual("user1@gmail.com", user.Email);
            Assert.AreEqual("city1", user.City);
            Assert.IsNull(user.Password);

            await _client.DeleteUser("user1");
        }

        [Test]
        public async void ShouldUpdateUser() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            await DeleteUserIfExists(_client, "user1");

            var user = new MyUsergridUser {UserName = "user1", Password = "user1", Email = "user1@gmail.com", City = "city1"};
            await _client.CreateUser(user);
            user = await _client.GetUser<MyUsergridUser>("user1");

            user.Email = "user-2@gmail.com";
            user.City = "city1-2";
            user.Password = "user1-2";
            await _client.UpdateUser(user);

            user = await _client.GetUser<MyUsergridUser>("user1");
            Assert.IsNotNull(user);
            Assert.AreEqual("user1", user.UserName);
            Assert.AreEqual("user-2@gmail.com", user.Email);
            Assert.AreEqual("city1-2", user.City);
            Assert.IsNull(user.Password);

            await _client.DeleteUser("user1");
        }
    }
}