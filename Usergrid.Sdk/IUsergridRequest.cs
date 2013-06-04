using RestSharp;

namespace Usergrid.Sdk
{
    public interface IUsergridRequest
    {
        IRestResponse<T> Execute<T>(string resource, Method method, object body = null) where T : new();
        IRestResponse Execute(string resource, Method method, object body = null);
        string AccessToken { get; set; }
    }
}