using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Usergrid.Sdk.Model;

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
        public string Path { get; internal set; }
    }
}