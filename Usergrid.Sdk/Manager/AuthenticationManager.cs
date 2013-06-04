using System;
using Usergrid.Sdk.Payload;
using Usergrid.Sdk.Model;
using RestSharp;

namespace Usergrid.Sdk.Manager
{
	internal class AuthenticationManager : ManagerBase, IAuthenticationManager
	{
		public AuthenticationManager (IUsergridRequest request) : base(request) {}

		public void ChangePassword (string userName, string oldPassword, string newPassword)
		{
			var payload = new ChangePasswordPayload {OldPassword = oldPassword, NewPassword = newPassword};
			var response = Request.Execute(string.Format("/users/{0}/password", userName), Method.POST, payload);
			ValidateResponse(response);
		}

		public void Login (string loginId, string secret, Usergrid.Sdk.Model.AuthType authType)
		{
			if (authType == AuthType.None)
			{
				Request.AccessToken = null;
				return;
			}

			var body = GetLoginBody(loginId, secret, authType);

			var response = Request.Execute<LoginResponse>("/token", Method.POST, body);
			ValidateResponse(response);

			Request.AccessToken = response.Data.AccessToken;
		}

		private object GetLoginBody(string loginId, string secret, AuthType authType)
		{
			object body = null;

			if (authType == AuthType.ClientId || authType == AuthType.Application)
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
