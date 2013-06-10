using System;
using System.Collections.Generic;
using System.Dynamic;
using RestSharp;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Model
{

    public class AppleNotification : Notification
    {
        public AppleNotification(string notifierIdentifier, string message, string sound = null) : base(notifierIdentifier, message)
        {
            Sound = sound;
        }

        public string Sound { get; set; }

        internal override object GetPayload()
        {
//            dynamic parent = new ExpandoObject();
//            dynamic child = new ExpandoObject();
//            dynamic aps = new ExpandoObject();
//
//            var parentDict = parent as IDictionary<string, object>;
//            var childDict = child as IDictionary<string, object>;
//            var apsDict = aps as IDictionary<string, object>;
//
//            if (Sound != null)
//            {
//                childDict.Add("alert", Message);
//                childDict.Add("sound", Sound);
//
//                apsDict.Add("abs", childDict);
//
//                parentDict.Add(NotifierIdentifier, apsDict);
//            }
//            else
//            {
//                parentDict.Add(NotifierIdentifier, Message);
//            }

                        if (Sound != null)
                        {
                            return new {aps = new {alert = Message, sound = Sound}};
                        }
                        else
                        {
                            return Message;
                        }
        }
    }
    
}