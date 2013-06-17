using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests
{
	[TestFixture]
	public class ActivitiesTests : BaseTest
	{
		[Test]
		public void ShouldCreateAndRetrieveUserActivities()
		{
		    var client = InitializeClientAndLogin(AuthType.ClientId);

			// Create a user
			var username = "user-" + GetRandomInteger (1, 100);
			var userEntity = new MyUsergridUser { UserName =  username};

			// See if this user exists
			var userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Delete if exists
			if (userFromUsergrid != null)
			{
				client.DeleteUser (username);
			}

			// Now create the user
			client.CreateUser (userEntity);

			userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Create an activity for this user
			var activityEntity = new UsergridActivity {
				Actor = new UsergridActor
				{
					DisplayName = "Joe Doe",
					Email = userFromUsergrid.Email,
					UserName = userFromUsergrid.UserName,
					Uuid = userFromUsergrid.Uuid,
					Image = new UsergridImage
					{
						Height = 10,
						Width = 20,
						Duration = 0,
						Url = "apigee.com"
					}
				},
				Content = "Hello Usergrid",
				Verb = "post"
			};

			client.PostActivity (userFromUsergrid.UserName, activityEntity);

			// Get the activities
			var activities = client.GetUserActivities<UsergridActivity> (userFromUsergrid.UserName);
			Assert.IsNotNull (activities);
			Assert.AreEqual (1, activities.Count);
			var thisActivity = activities [0].Entity;
			Assert.AreEqual ("Joe Doe", thisActivity.Actor.DisplayName);
			Assert.AreEqual (userFromUsergrid.Email, thisActivity.Actor.Email);
			Assert.AreEqual (10, thisActivity.Actor.Image.Height);
			Assert.AreEqual (20, thisActivity.Actor.Image.Width);
			Assert.AreEqual ("Hello Usergrid", thisActivity.Content);
			Assert.IsTrue (thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

			// Get the feed
			var feed = client.GetUserFeed<UsergridActivity> (userFromUsergrid.UserName);
			Assert.IsNotNull (feed);
			Assert.AreEqual (1, feed.Count);
			thisActivity = feed [0].Entity;
			Assert.AreEqual ("Joe Doe", thisActivity.Actor.DisplayName);
			Assert.AreEqual (userFromUsergrid.Email, thisActivity.Actor.Email);
			Assert.AreEqual (10, thisActivity.Actor.Image.Height);
			Assert.AreEqual (20, thisActivity.Actor.Image.Width);
			Assert.AreEqual ("Hello Usergrid", thisActivity.Content);
			Assert.IsTrue (thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

			// Delete user
			client.DeleteUser (username);

			// See if this user exists
			userFromUsergrid = client.GetUser<UsergridUser> (username);
			Assert.IsNull (userFromUsergrid);
		}

		[Test]
		public void ShouldCreateAndRetrieveGroupActivities()
		{
			var client = new Client(Organization, Application);
			client.Login(ClientId, ClientSecret, AuthType.ClientId);

			// Create a user
			var username = "user-" + GetRandomInteger (1, 100);
			var userEntity = new MyUsergridUser { UserName =  username};

			// See if this user exists
			var userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Delete if exists
			if (userFromUsergrid != null)
			{
				client.DeleteUser (username);
			}

			// Now create the user
			client.CreateUser (userEntity);

			userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Create a group
			var groupName = "group-" + GetRandomInteger (1, 100);
			var groupEntity = new UsergridGroup { Path = groupName, Title = "mygrouptitle" };

			// See if this group exists
			var groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);

			// Delete if exists
			if (groupFromUsergrid != null)
			{
				client.DeleteGroup (groupName);
			}

			// Now create the group
			client.CreateGroup (groupEntity);

			groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);

			// Create an activity for this group
			var activityEntity = new UsergridActivity {
				Actor = new UsergridActor
				{
					DisplayName = "Joe Doe",
					Email = userFromUsergrid.Email,
					UserName = userFromUsergrid.UserName,
					Uuid = userFromUsergrid.Uuid,
					Image = new UsergridImage
					{
						Height = 10,
						Width = 20,
						Duration = 0,
						Url = "apigee.com"
					}
				},
				Content = "Hello Usergrid",
				Verb = "post"
			};

			client.PostActivityToGroup (groupName, activityEntity);

			// Get the activities
			var activities = client.GetGroupActivities<UsergridActivity> (groupName);
			Assert.IsNotNull (activities);
			Assert.AreEqual (1, activities.Count);
			var thisActivity = activities [0].Entity;
			Assert.AreEqual ("Joe Doe", thisActivity.Actor.DisplayName);
			Assert.AreEqual (userFromUsergrid.Email, thisActivity.Actor.Email);
			Assert.AreEqual (10, thisActivity.Actor.Image.Height);
			Assert.AreEqual (20, thisActivity.Actor.Image.Width);
			Assert.AreEqual ("Hello Usergrid", thisActivity.Content);
			Assert.IsTrue (thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

			// Get the feed
			var feed = client.GetGroupFeed<UsergridActivity> (groupName);
			Assert.IsNotNull (feed);
			Assert.AreEqual (1, feed.Count);
			thisActivity = feed [0].Entity;
			Assert.AreEqual ("Joe Doe", thisActivity.Actor.DisplayName);
			Assert.AreEqual (userFromUsergrid.Email, thisActivity.Actor.Email);
			Assert.AreEqual (10, thisActivity.Actor.Image.Height);
			Assert.AreEqual (20, thisActivity.Actor.Image.Width);
			Assert.AreEqual ("Hello Usergrid", thisActivity.Content);
			Assert.IsTrue (thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

			// Delete user
			client.DeleteUser (username);

			// See if this user exists
			userFromUsergrid = client.GetUser<UsergridUser> (username);
			Assert.IsNull (userFromUsergrid);

			// Delete group
			client.DeleteGroup (groupName);

			// See if this group exists
			groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);
			Assert.IsNull (groupFromUsergrid);
		}
	
		[Test]
		public void ShouldCreateAndRetrieveUsersFollowersActivities()
		{
			var client = new Client(Organization, Application);
			client.Login(ClientId, ClientSecret, AuthType.ClientId);

			// Create a user
			var username = "user-" + GetRandomInteger (1, 100);
			var userEntity = new MyUsergridUser { UserName =  username, Name = "Joe Doe"};

			// See if this user exists
			var userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Delete if exists
			if (userFromUsergrid != null)
			{
				client.DeleteUser (username);
			}

			// Now create the user
			client.CreateUser (userEntity);

			userFromUsergrid = client.GetUser<UsergridUser> (username);

			// Create a group
			var groupName = "group-" + GetRandomInteger (1, 100);
			var groupEntity = new UsergridGroup { Path = groupName, Title = "mygrouptitle" };

			// See if this group exists
			var groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);

			// Delete if exists
			if (groupFromUsergrid != null)
			{
				client.DeleteGroup (groupName);
			}

			// Now create the group
			client.CreateGroup (groupEntity);

			groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);

			// Create an activity for this group
			var activityEntity = new UsergridActivity (userFromUsergrid) {
				Content = "Hello Usergrid",
				Verb = "post"
			};

			client.PostActivityToUsersFollowersInGroup (username, groupName, activityEntity);

			// Get the activities
			var activities = client.GetUserActivities<UsergridActivity> (username);
			Assert.IsNotNull (activities);
			Assert.AreEqual (1, activities.Count);
			var thisActivity = activities [0].Entity;
			Assert.AreEqual (userFromUsergrid.Name, thisActivity.Actor.DisplayName);
			Assert.AreEqual (userFromUsergrid.Email, thisActivity.Actor.Email);
			Assert.IsNull (thisActivity.Actor.Image);
			Assert.AreEqual ("Hello Usergrid", thisActivity.Content);
			Assert.IsTrue (thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

			// Delete user
			client.DeleteUser (username);

			// See if this user exists
			userFromUsergrid = client.GetUser<UsergridUser> (username);
			Assert.IsNull (userFromUsergrid);

			// Delete group
			client.DeleteGroup (groupName);

			// See if this group exists
			groupFromUsergrid = client.GetGroup<UsergridGroup> (groupName);
			Assert.IsNull (groupFromUsergrid);
		}
	}

}

