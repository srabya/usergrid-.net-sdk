using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Manager
{
    internal class AuthenticationManager : ManagerBase, IAuthenticationManager
    {
        public AuthenticationManager(IUsergridRequest request) : base(request)
        {
        }

        public async Task ChangePassword(string userName, string oldPassword, string newPassword)
        {
            var payload = new ChangePasswordPayload {OldPassword = oldPassword, NewPassword = newPassword};
            IRestResponse response = await Request.ExecuteJsonRequest(string.Format("/users/{0}/password", userName), HttpMethod.Post, payload);
            ValidateResponse(response);
        }

        public async Task Login(string loginId, string secret, AuthType authType)
        {
            if (authType == AuthType.None)
            {
                Request.AccessToken = null;
                return;
            }

            object body = GetLoginBody(loginId, secret, authType);

            IRestResponse response = await Request.ExecuteJsonRequest("/token", HttpMethod.Post, body);
            ValidateResponse(response);
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content);

            Request.AccessToken = loginResponse.AccessToken;
        }

        private object GetLoginBody(string loginId, string secret, AuthType authType)
        {
            object body = null;

            if (authType == AuthType.Organization || authType == AuthType.Application)
            {
                body = new ClientIdLoginPayload
                    {
                        ClientId = loginId,
                        ClientSecret = secret
                    };
            }
            else if (authType == AuthType.User)
            {
                body = new UserLoginPayload
                    {
                        UserName = loginId,
                        Password = secret
                    };
            }
            return body;
        }
    }
}