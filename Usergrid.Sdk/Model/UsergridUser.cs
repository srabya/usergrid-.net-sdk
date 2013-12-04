using Newtonsoft.Json;

namespace Usergrid.Sdk.Model
{
    public class UsergridUser : UsergridEntity
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}