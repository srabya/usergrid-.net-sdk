using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;
using System.Runtime.Serialization;
using System;

namespace Usergrid.Sdk.Model
{
	public class UsergridEntity<T> : UsergridEntity
	{
		public T Entity {get;set;}
	}

    public class UsergridEntity
    {
        public string Uuid { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }

		[JsonProperty("created")]
		private long createdLong {  get;  set;}

		[JsonIgnore]
		public DateTime CreatedDate {
			get { return createdLong.FromUnixTime(); }
		}

		[JsonProperty("modified")]
		private long modifiedLong {  get;  set;}

		[JsonIgnore]
		public DateTime ModifiedDate {
			get { return modifiedLong.FromUnixTime(); }
		}

    }
}