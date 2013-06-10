using System;

namespace Usergrid.Sdk.Model
{
	public class NotificationRecipients : INotificationRecipients
	{
		string userName;
		string userUuid;
		string userQuery;
		string groupPath;
		string groupQuery;
		string deviceName;
		string deviceQuery;

		public INotificationRecipients AddUserWithName (string name)
		{
			userName = name;
			return this;
		}

		public INotificationRecipients AddUserWithUuid (string uuid)
		{
			if (userName != null)
				throw new ArgumentException ();

			userUuid = uuid;
			return this;
		}

		public INotificationRecipients AddUserWithQuery (string query)
		{
			userQuery = query;
			return this;
		}

		public INotificationRecipients AddGroupWithPath (string path)
		{
			groupPath = path;
			return this;
		}

		public INotificationRecipients AddGroupWithQuery (string query)
		{
			groupQuery = query;
			return this;
		}

		public INotificationRecipients AddDeviceWithName (string name)
		{
			deviceName = name;
			return this;
		}

		public INotificationRecipients AddDeviceWithQuery (string query)
		{
			deviceQuery = query;
			return this;
		}

		public string BuildQuery()
		{
			var query = string.Empty;

			if (groupPath != null)
			{
				query += string.Format ("/groups/{0}", groupPath);
			}

			if (groupQuery != null)
			{
				query += string.Format ("/groups;ql={0}", groupQuery);
			}

			if (userName != null)
			{
				query += string.Format ("/users/{0}", userName);
			}

			if (userUuid != null)
			{
				query += string.Format ("/users/{0}", userUuid);
			}

			if (userQuery != null)
			{
				query += string.Format ("/users;ql={0}", userQuery);
			}

			if (deviceName != null)
			{
				query += string.Format ("/devices/{0}", deviceName);
			}

			if (deviceQuery != null)
			{
				query += string.Format ("/devices;ql={0}", deviceQuery);
			}

			query += "/notifications";

			return query;
		}
	}
}

