
namespace Usergrid.Sdk.Model
{

    public class AppleNotification : Notification
    {
        public AppleNotification(string notifierIdentifier, string message, string sound = null)
            : base(notifierIdentifier, message)
        {
            Sound = sound;
        }

        public string Sound { get; set; }

        internal override object GetPayload()
        {

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