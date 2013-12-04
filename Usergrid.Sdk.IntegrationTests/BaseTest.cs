using System;
using System.Configuration;
using System.Threading.Tasks;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    public class BaseTest {
        private static readonly Random random = new Random(DateTime.Now.Millisecond);

        private readonly Configuration _config;

        public BaseTest() {
            var configMap = new ExeConfigurationFileMap {ExeConfigFilename = "MySettings.config"};
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            if (config.HasFile && config.AppSettings.Settings.Count > 0)
                _config = config;
        }

        protected string Organization {
            get { return GetAppSetting("organization"); }
        }

        protected string Application {
            get { return GetAppSetting("application"); }
        }

        protected string ClientId {
            get { return GetAppSetting("clientId"); }
        }

        protected string ClientSecret {
            get { return GetAppSetting("clientSecret"); }
        }

        protected string ApplicationId {
            get { return GetAppSetting("applicationId"); }
        }

        protected string ApplicationSecret {
            get { return GetAppSetting("applicationSecret"); }
        }

        protected string UserId {
            get { return GetAppSetting("userId"); }
        }

        protected string UserSecret {
            get { return GetAppSetting("userSecret"); }
        }

        protected string P12CertificatePath {
            get { return GetAppSetting("p12CertificatePath"); }
        }

        protected string GoogleApiKey {
            get { return GetAppSetting("googleApiKey"); }
        }

        private string GetAppSetting(string key) {
            return _config == null ? ConfigurationManager.AppSettings[key] : _config.AppSettings.Settings[key].Value;
        }

        protected async Task<IClient> InitializeClientAndLogin(AuthType authType) {
            var client = new Client(Organization, Application);
            if (authType == AuthType.Application || authType == AuthType.Organization)
                await client.Login(ClientId, ClientSecret, authType);
            else if (authType == AuthType.User)
                await client.Login(UserId, UserSecret, authType);

            return client;
        }

        protected static int GetRandomInteger(int minValue, int maxValue) {
            return random.Next(minValue, maxValue);
        }

        protected async Task DeleteEntityIfExists<TEntity>(IClient client, string collectionName, string entityIdentifier) {
            TEntity customer = await client.GetEntity<TEntity>(collectionName, entityIdentifier);

            if (customer != null)
                await client.DeleteEntity(collectionName, entityIdentifier);
        }

        protected async Task DeleteDeviceIfExists(IClient client, string deviceName) {
            UsergridDevice usergridDevice = await client.GetDevice<UsergridDevice>(deviceName);
            if (usergridDevice != null)
                await client.DeleteDevice(usergridDevice.Uuid);
        }

        protected async Task DeleteUserIfExists(IClient client, string userName) {
            UsergridUser existingUser = await client.GetUser<UsergridUser>(userName);
            if (existingUser != null)
                await client.DeleteUser(existingUser.Uuid);
        }

        protected async Task<UsergridUser> SetupUsergridUser(IClient client, UsergridUser user) {
            await DeleteUserIfExists(client, user.UserName);
            await client.CreateUser(user);
            return await client.GetUser<UsergridUser>(user.UserName);
        }

        protected async Task<UsergridGroup> SetupUsergridGroup(IClient client, UsergridGroup @group) {
            var existingGroup = await client.GetGroup<UsergridGroup>(@group.Path);
            if (existingGroup != null)
                await client.DeleteGroup(existingGroup.Path);
            await client.CreateGroup(@group);
            return await client.GetGroup<UsergridGroup>(@group.Path);
        }
    }
}