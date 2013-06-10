using System.Net;
using NSubstitute;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests
{
	internal static class Helpers
    {
		internal static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

		internal static IRestResponse<T> SetUpRestResponseWithContent<T>(HttpStatusCode httpStatusCode, object responseContent)
        {
            var restResponse = Substitute.For<IRestResponse<T>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Content.Returns(responseContent.Serialize());
            return restResponse;
        }

		internal static IRestResponse<T> SetUpRestResponseWithData<T>(HttpStatusCode httpStatusCode, T responseData)
        {
            var restResponse = Substitute.For<IRestResponse<T>>();
            restResponse.StatusCode.Returns(HttpStatusCode.OK);
            restResponse.Data.Returns(responseData);
            return restResponse;
        }

		internal static IUsergridRequest InitializeUserGridRequestWithAccessToken(string accessToken)
        {
            IRestResponse<LoginResponse> loginResponse = SetUpRestResponseWithData(HttpStatusCode.OK, new LoginResponse {AccessToken = accessToken});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest<LoginResponse>(Arg.Any<string>(), Arg.Any<Method>(), Arg.Any<object>())
                .Returns(loginResponse);

            return request;
        }
    }
}