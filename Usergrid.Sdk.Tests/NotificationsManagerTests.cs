using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;
using Usergrid.Sdk.Payload;

namespace Usergrid.Sdk.Tests
{
    [TestFixture]
    public class NotificationsManagerTests
    {
        private IUsergridRequest _request;
        private NotificationsManager _notificationsManager;

        [SetUp]
        public void Setup()
        {
            _request = Substitute.For<IUsergridRequest>();
            _notificationsManager = new NotificationsManager(_request);
        }


        [Test]
        public void CreateNotifierForAppleExecutesJsonRequestWithCorrectParameters()
        {
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);
            _request.ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var p12Certificate = new byte[0];
            _notificationsManager.CreateNotifierForApple("notifierName", "development", p12Certificate);

            _request.Received(1).ExecuteJsonRequest(
                "/notifiers",
                HttpMethod.Post,
                Arg.Is<AppleNotifierPayload>(d => d.Name == "notifierName" && d.Provider == "apple" && d.Environment == "development" && d.p12Certificate == p12Certificate));
        }

        [Test]
        public void CreateNotifierForAndroidExecutesJsonRequestWithCorrectParameters()
        {
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);
            _request.ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            _notificationsManager.CreateNotifierForAndroid("notifierName", "apiKey");

            _request.Received(1).ExecuteJsonRequest(
                "/notifiers",
                HttpMethod.Post,
                Arg.Is<AndroidNotifierPayload>(p => p.ApiKey == "apiKey" && p.Name == "notifierName" && p.Provider == "google"));
        }

        [Test]
        public void PublishNotificationPostsByBuildingQueryAndPayload()
        {
            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);
            _request.ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var recipients = Substitute.For<INotificationRecipients>();
            recipients.BuildQuery().Returns("query");

            var appleNotification = new AppleNotification("appleNotifierName", "appleTestMessge", "chime");
            var googleNotification = new AndroidNotification("googleNotifierName", "androidTestMessage");

            var deliverAt = DateTime.Now.AddDays(1);
            var expireAt = DateTime.Now.AddDays(2);

            var schedulerSettings = new NotificationSchedulerSettings {DeliverAt = deliverAt, ExpireAt = expireAt};

            //call PublishNotification
            _notificationsManager.PublishNotification(new Notification[] {appleNotification, googleNotification}, recipients, schedulerSettings);

            //assert
            recipients.Received(1).BuildQuery();

            Func<NotificationPayload, bool> validatePayload = p =>
                                                                  {
                                                                      bool isValid = true;
                                                                      isValid &= p.DeliverAt == deliverAt.ToUnixTime();
                                                                      isValid &= p.ExpireAt == expireAt.ToUnixTime();

                                                                      var applePayload = p.Payloads["appleNotifierName"].GetReflectedProperty("aps");
                                                                      isValid &= (string) applePayload.GetReflectedProperty("alert") == "appleTestMessge";
                                                                      isValid &= (string) applePayload.GetReflectedProperty("sound") == "chime";

                                                                      var googlePayload = p.Payloads["googleNotifierName"].GetReflectedProperty("data");
                                                                      isValid &= (string) googlePayload == "androidTestMessage";

                                                                      return isValid;
                                                                  };
            _request.Received(1).ExecuteJsonRequest("query", HttpMethod.Post,
                                                    Arg.Is<NotificationPayload>(
                                                        p => validatePayload(p)));
        }
    }
}