using Newtonsoft.Json;

namespace Usergrid.Sdk.Payload
{
    internal class AndroidNotifierPayload
    {
        public string Name { get; set; }

        public string Provider
        {
            get { return "google"; }
        }

        public string ApiKey { get; set; }
    }
    
    internal class AppleNotifierPayload
    {
        public string Name { get; set; }

        public string Provider
        {
            get { return "apple"; }
        }

        public string Environment { get; set; }
        [JsonProperty(PropertyName = "p12Certificate")]
        public byte[] p12Certificate { get; set; }
    }
}