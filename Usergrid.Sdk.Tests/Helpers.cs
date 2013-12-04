using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using NSubstitute;
using Newtonsoft.Json;
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

		internal static IRestResponse SetUpRestResponseWithContent(HttpStatusCode httpStatusCode, object responseContent)
		{
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(httpStatusCode);
            restResponse.Content.Returns(responseContent is string ? responseContent : responseContent.Serialize());
            return restResponse;
		}

        internal static IRestResponse SetUpRestResponse(HttpStatusCode httpStatusCode)
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(httpStatusCode);
            return restResponse;
        }

		internal static IRestResponse SetUpRestResponseWithData(HttpStatusCode httpStatusCode, object responseData)
        {
            var restResponse = Substitute.For<IRestResponse>();
            restResponse.StatusCode.Returns(httpStatusCode);
            restResponse.Content.Returns(JsonConvert.SerializeObject(responseData));
            return restResponse;
        }
//
        internal static IUsergridRequest SetUpUsergridRequestWithRestResponse(IRestResponse restResponse)
        {
            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            return request;
        }

		internal static IUsergridRequest InitializeUserGridRequestWithAccessToken(string accessToken)
        {
            IRestResponse loginResponse = SetUpRestResponseWithData(HttpStatusCode.OK, new LoginResponse {AccessToken = accessToken});

            var request = Substitute.For<IUsergridRequest>();
            request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(loginResponse));

            return request;
        }

        public static object GetReflectedProperty(this object obj, string propertyName)
        {
            PropertyInfo property = obj.GetType().GetProperty(propertyName);

            if (property == null)
                return null;

            return property.GetValue(obj, null);
        }
    }
}