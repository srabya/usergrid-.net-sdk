using System;
using Newtonsoft.Json;

namespace Usergrid.Sdk
{
	public class UsergridActor
	{
        [JsonProperty("displayname")]
        public string DisplayName {get;set;}
        public string Uuid { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("image")]
        public UsergridImage Image { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
	}
}

