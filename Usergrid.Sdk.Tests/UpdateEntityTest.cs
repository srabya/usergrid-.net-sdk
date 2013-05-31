using System.Net;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests
{
	[TestFixture]
	public class UpdateEntityTest
	{
		[Test]
		public void ShouldPutToCorrectEndPoint()
		{
			var collectionName = "collection";
			var entityName = "entity";
			var entityToPost = new { FirstName = "first", LastName = "last" };

			var restResponse = Substitute.For<IRestResponse>();
			restResponse.StatusCode.Returns(HttpStatusCode.OK);

			var request = Substitute.For<IUsergridRequest>();
			request
				.Execute (Arg.Any<string>(), Arg.Any<Method> (), Arg.Any<object> (), Arg.Any<string> ())
					.Returns(restResponse);

			var client = new Client(null, null, request: request);
			client.UpdateEntity (collectionName, entityName, entityToPost);

			request.Received(1).Execute(
				Arg.Is(string.Format("/{0}/{1}", collectionName, entityName)), 
				Arg.Is(Method.PUT), 
				Arg.Is(entityToPost), 
				Arg.Any<string>());
		}

		[Test]
		public void ShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
		{
			string restResponseContent = new UsergridError {
				Description = "Service resource not found", 
				Error = "service_resource_not_found"}
			.Serialize();

			var collectionName = "collection";
			var entityName = "entity";
			var entityToPost = new { FirstName = "first", LastName = "last" };

			var restResponse = Substitute.For<IRestResponse>();
			restResponse.StatusCode.Returns(HttpStatusCode.NotFound);
			restResponse.Content.Returns(restResponseContent);

			var request = Substitute.For<IUsergridRequest>();
			request
				.Execute (Arg.Any<string>(), Arg.Any<Method> (), Arg.Any<object> (), Arg.Any<string> ())
					.Returns(restResponse);

			var client = new Client(null, null, request: request);
			try
			{
				client.UpdateEntity(collectionName, entityName, entityToPost);
				new AssertionException("UserGridException was expected to be thrown here");
			} catch (UsergridException e)
			{
				Assert.AreEqual ("service_resource_not_found", e.ErrorCode);
				Assert.AreEqual ("Service resource not found", e.Message);
			}
		}

		[Test]
		public void ShouldPassCorrectAccessToken()
		{
            const string accessToken = "access_token";
            var request = Helpers.InitializeUserGridRequestWithAccessToken(accessToken);
		    var collectionName = "collection";
			var entityName = "entity";
			var entityToPost = new { FirstName = "first", LastName = "last" };

			var restResponse = Substitute.For<IRestResponse>();
			restResponse.StatusCode.Returns(HttpStatusCode.OK);

			request
				.Execute (Arg.Any<string>(), Arg.Any<Method> (), Arg.Any<object> (), Arg.Any<string> ())
					.Returns (restResponse);

			var client = new Client (null, null, request: request);
			client.Login (null, null, AuthType.ClientId);
			client.UpdateEntity (collectionName, entityName, entityToPost);

			request.Received (1).Execute (
				Arg.Is (string.Format("/{0}/{1}", collectionName, entityName)), 
				Arg.Is (Method.PUT), 
				Arg.Is (entityToPost), 
				accessToken);
		}
	}
}

