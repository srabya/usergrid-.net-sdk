using Newtonsoft.Json;

namespace Usergrid.Sdk.Model
{
	public class Group
	{
		[JsonProperty("path")]
		public string Path {get;set;}

		[JsonProperty("title")]
		public string Title { get; set;}
	}
}

