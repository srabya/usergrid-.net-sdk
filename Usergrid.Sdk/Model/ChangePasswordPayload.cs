using Newtonsoft.Json;

namespace Usergrid.Sdk.Model
{
    public class ChangePasswordPayload
    {
        [JsonProperty(PropertyName = "oldpassword")]
        public string OldPassword { get; set; }

        [JsonProperty(PropertyName = "newpassword")]
        public string NewPassword { get; set; }
    }
}