using System;
using Newtonsoft.Json;

namespace Usergrid.Sdk.Model
{
	public class UsergridActivity
	{
		public UsergridActivity(){}

		public UsergridActivity(UsergridUser user, UsergridImage image = null){
			Actor = new UsergridActor {
				DisplayName = user.Name,
				Email = user.Email,
				Image = image,
				UserName = user.UserName,
				Uuid = user.Uuid
			};
		}

        [JsonProperty("actor")]
		public UsergridActor Actor {get;set;}

        [JsonProperty("verb")]
        public string Verb { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

		[JsonProperty("published")]
		private long PublishedLong {  get;  set;}

		[JsonIgnore]
		public DateTime PublishedDate {
			get { return PublishedLong.FromUnixTime(); }
		}
	}
}

