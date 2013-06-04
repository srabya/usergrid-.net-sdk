using RestSharp;

namespace Usergrid.Sdk
{
    internal class UsergridRequest : IUsergridRequest
    {
        private readonly string _organization;
        private readonly string _application;
        private readonly RestClient _restClient;

        public string AccessToken { get; set; }

        public UsergridRequest(string baseUri, string organization, string application)
        {
            _organization = organization;
            _application = application;
            _restClient = new RestClient(baseUri);
        }

        public IRestResponse<T> Execute<T>(string resource, Method method, object body = null) where T : new()
        {
            var request = GetRequest(resource, method, body);
            IRestResponse<T> response = _restClient.Execute<T>(request);
            return    response;
        }

        public IRestResponse Execute(string resource, Method method, object body = null) 
        {
            var request = GetRequest(resource, method, body);
            IRestResponse response = _restClient.Execute(request);
            return    response;
        }

        private RestRequest GetRequest(string resource, Method method, object body)
        {
            var request = new RestRequest(string.Format("{0}/{1}{2}", _organization, _application, resource), method)
                              {
                                  JsonSerializer = new RestSharpJsonSerializer()
                              };
            AddAuthorizationHeader(request);
			if (body != null)
			{
				request.RequestFormat = DataFormat.Json;
            	request.AddBody(body);
			}
            return request;
        }

        private void AddAuthorizationHeader(RestRequest request)
        {
            if (AccessToken != null)
                request.AddHeader("Authorization", string.Format("Bearer {0}", AccessToken));
        }

    }
}