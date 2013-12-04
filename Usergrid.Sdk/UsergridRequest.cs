using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Usergrid.Sdk.Manager;

namespace Usergrid.Sdk
{
    internal class UsergridRequest : IUsergridRequest
    {
        private readonly string _application;
        private readonly string _organization;
        private readonly HttpClient _restClient;

        public UsergridRequest(string baseUri, string organization, string application, HttpClient restClient = null)
        {
            _organization = organization;
            _application = application;
            _restClient = restClient ?? new HttpClient();
            _restClient.BaseAddress = new Uri(baseUri);
        }

        public string AccessToken { get; set; }

        public async Task<IRestResponse> ExecuteJsonRequest(string resource, HttpMethod method, object body = null)
        {
            HttpRequestMessage request = GetRequest(resource, method);
            AddBodyAsJson(body, request);
            HttpResponseMessage response = await _restClient.SendAsync(request);
            return new RestResponse(await response.Content.ReadAsStringAsync(), response);
        }

        private HttpRequestMessage GetRequest(string resource, HttpMethod method)
        {
            var request = new HttpRequestMessage(method,  string.Format("{0}/{1}{2}", _organization, _application, resource));
            AddAuthorizationHeader(request);
            return request;
        }

        private static void AddBodyAsJson(object body, HttpRequestMessage request)
        {
            if (body != null) {
                var content = JsonConvert.SerializeObject(body, new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if (AccessToken != null)
                request.Headers.Add("Authorization", string.Format("Bearer {0}", AccessToken));
        }
    }
}