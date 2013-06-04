using Newtonsoft.Json;

namespace Usergrid.Sdk.Payload
{
    public class LoginResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        //todo: expires_in
    }
}