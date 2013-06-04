using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
    [TestFixture]
    public class LoginTests : BaseTest
    {
        [Test]
        public void ShouldLoginSuccessfullyWithClientCredentials()
        {
            var client = new Client(Organization, Application);
            client.Login(ClientId, ClientSecret, AuthType.ClientId);
        }

        [Test]
        public void ShouldThrowWithInvalidUserCredentials()
        {
            var client = new Client(Organization, Application);

            try
            {
                client.Login("Invalid_User_Name", "Invalid_Password", AuthType.User);
                Assert.True(true, "Was expecting login to throw UserGridException");
            }
            catch (UsergridException e)
            {
                Assert.That(e.ErrorCode, Is.EqualTo("invalid_grant"));
            }
        }
    }
}