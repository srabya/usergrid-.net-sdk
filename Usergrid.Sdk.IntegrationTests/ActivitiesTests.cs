using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    [TestFixture]
    public class ActivitiesTests : BaseTest {
        private IClient _client;

        [Test]
        public async void ShouldCreateAndRetrieveGroupActivities() {
            _client = await InitializeClientAndLogin(AuthType.Organization);

            // Create a user
            UsergridUser usergridUser = await SetupUsergridUser(_client, new UsergridUser {UserName = "test_user", Email = "test_user@apigee.com"});
            // Create a group
            UsergridGroup usergridGroup = await SetupUsergridGroup(_client, new UsergridGroup {Path = "test-group", Title = "mygrouptitle"});

            // Create an activity for this group
            var activityEntity = new UsergridActivity
                {
                    Actor = new UsergridActor
                        {
                            DisplayName = "Joe Doe",
                            Email = usergridUser.Email,
                            UserName = usergridUser.UserName,
                            Uuid = usergridUser.Uuid,
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

            await _client.PostActivityToGroup(usergridGroup.Path, activityEntity);

            // Get the activities
            var activities = await _client.GetGroupActivities<UsergridActivity>(usergridGroup.Path);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count);
            var thisActivity = activities[0];
            Assert.AreEqual("Joe Doe", thisActivity.Actor.DisplayName);
            Assert.AreEqual(usergridUser.Email, thisActivity.Actor.Email);
            Assert.AreEqual(10, thisActivity.Actor.Image.Height);
            Assert.AreEqual(20, thisActivity.Actor.Image.Width);
            Assert.AreEqual("Hello Usergrid", thisActivity.Content);
            Assert.IsTrue(thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

            // Get the feed
            var feed = await _client.GetGroupFeed<UsergridActivity>(usergridGroup.Path);
            Assert.IsNotNull(feed);
            Assert.AreEqual(1, feed.Count);
            thisActivity = feed[0];
            Assert.AreEqual("Joe Doe", thisActivity.Actor.DisplayName);
            Assert.AreEqual(usergridUser.Email, thisActivity.Actor.Email);
            Assert.AreEqual(10, thisActivity.Actor.Image.Height);
            Assert.AreEqual(20, thisActivity.Actor.Image.Width);
            Assert.AreEqual("Hello Usergrid", thisActivity.Content);
            Assert.IsTrue(thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));
        }

        [Test]
        public async void ShouldCreateAndRetrieveUserActivities() {
            _client = await InitializeClientAndLogin(AuthType.Organization);
            // Create a user
            UsergridUser usergridUser = await SetupUsergridUser(_client, new UsergridUser {UserName = "test_user", Email = "test_user@apigee.com"});
            // Create an activity for this user
            var activityEntity = new UsergridActivity
                {
                    Actor = new UsergridActor
                        {
                            DisplayName = "Joe Doe",
                            Email = usergridUser.Email,
                            UserName = usergridUser.UserName,
                            Uuid = usergridUser.Uuid,
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

            await _client.PostActivity(usergridUser.UserName, activityEntity);

            // Get the activities
            var activities = await _client.GetUserActivities<UsergridActivity>(usergridUser.UserName);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count);
            var thisActivity = activities[0];
            Assert.AreEqual("Joe Doe", thisActivity.Actor.DisplayName);
            Assert.AreEqual(usergridUser.Email, thisActivity.Actor.Email);
            Assert.AreEqual(10, thisActivity.Actor.Image.Height);
            Assert.AreEqual(20, thisActivity.Actor.Image.Width);
            Assert.AreEqual("Hello Usergrid", thisActivity.Content);
            Assert.IsTrue(thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));

            // Get the feed
            var feed = await _client.GetUserFeed<UsergridActivity>(usergridUser.UserName);
            Assert.IsNotNull(feed);
            Assert.AreEqual(1, feed.Count);
            thisActivity = feed[0];
            Assert.AreEqual("Joe Doe", thisActivity.Actor.DisplayName);
            Assert.AreEqual(usergridUser.Email, thisActivity.Actor.Email);
            Assert.AreEqual(10, thisActivity.Actor.Image.Height);
            Assert.AreEqual(20, thisActivity.Actor.Image.Width);
            Assert.AreEqual("Hello Usergrid", thisActivity.Content);
            Assert.IsTrue(thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));
        }

        [Test]
        public async void ShouldCreateAndRetrieveUsersFollowersActivities() {
            _client = await InitializeClientAndLogin(AuthType.Organization);

            // Create a user
            UsergridUser usergridUser = await SetupUsergridUser(_client, new UsergridUser {UserName = "newtest_user", Email = "newtest_user@apigee.com"});
            // Create a group
            UsergridGroup usergridGroup = await SetupUsergridGroup(_client, new UsergridGroup {Path = "newtest-group", Title = "newTitle"});


            // Create an activity for this group
            var activityEntity = new UsergridActivity
                {
                    Actor = new UsergridActor
                        {
                            DisplayName = "Joe Doe",
                            Email = usergridUser.Email,
                            UserName = usergridUser.UserName,
                            Uuid = usergridUser.Uuid,
                            Image = new UsergridImage
                                {
                                    Height = 10,
                                    Width = 20,
                                    Duration = 0,
                                    Url = "apigee.com"
                                }
                        },
                    Content = "Hello to Usergrid2",
                    Verb = "post"
                };

            await _client.PostActivityToUsersFollowersInGroup(usergridUser.UserName, usergridGroup.Path, activityEntity);

            // Get the activities
            var activities = await _client.GetUserActivities<UsergridActivity>(usergridUser.UserName);
            Assert.IsNotNull(activities);
            Assert.AreEqual(1, activities.Count);
            var thisActivity = activities[0];
            Assert.AreEqual(usergridUser.Name, thisActivity.Actor.DisplayName);
            Assert.AreEqual(usergridUser.Email, thisActivity.Actor.Email);
            Assert.IsNull(thisActivity.Actor.Image);
            Assert.AreEqual("Hello to Usergrid2", thisActivity.Content);
            Assert.IsTrue(thisActivity.PublishedDate > DateTime.Now.ToUniversalTime().AddHours(-1));
        }
    }
}