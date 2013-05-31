using Newtonsoft.Json;

namespace Usergrid.Sdk.Model
{
    public class User
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
    }
}