﻿using System;
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
    public class EntityManagerTests
    {

        [SetUp]
        public void Setup()
        {
            _request = Substitute.For<IUsergridRequest>();
            _entityManager = new EntityManager(_request);
        }

        private IUsergridRequest _request;
        private EntityManager _entityManager;

        [Test]
        public async void CreateEntityShouldPostToCorrectEndPoint()
        {
            const string collectionName = "collection";
            var entityToPost = new Friend {Name = "name1", Age = 1};
            var restResponseContent = new UsergridGetResponse<Friend> {Entities = new List<Friend> {entityToPost}, Cursor = "cursor"};
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);
            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            Friend returnedEntity = await _entityManager.CreateEntity(collectionName, entityToPost);

            _request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}", collectionName)),
                Arg.Is(HttpMethod.Post),
                Arg.Is(entityToPost));

            Assert.AreEqual(entityToPost.Age, returnedEntity.Age);
            Assert.AreEqual(entityToPost.Name, returnedEntity.Name);
        }

        [Test]
        public async void CreateEntityShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsBadRequest()
        {
            var restResponseContent = new UsergridError
                                          {
                                              Description = "Subject does not have permission",
                                              Error = "unauthorized"
                                          };
            const string collectionName = "collection";
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.BadRequest, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            try
            {
                await _entityManager.CreateEntity(collectionName, new object());
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("unauthorized", e.ErrorCode);
                Assert.AreEqual("Subject does not have permission", e.Message);
            }
        }


        [Test]
        public void DeleteEntityShouldSendDeleteToCorrectEndPoint()
        {
            const string collection = "collection";
            const string identifier = "identifier";

            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            _entityManager.DeleteEntity(collection, identifier);

            _request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}/{1}", collection, identifier)),
                Arg.Is(HttpMethod.Delete),
                Arg.Any<object>());
        }

        [Test]
        public async void DeleteEntityShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsNotFound()
        {
            var restResponseContent = new UsergridError
                                          {
                                              Description = "Service resource not found",
                                              Error = "service_resource_not_found"
                                          };
            const string collection = "collection";
            const string identifier = "identifier";

            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.NotFound, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            try
            {
                await _entityManager.DeleteEntity(collection, identifier);
                throw new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("service_resource_not_found", e.ErrorCode);
                Assert.AreEqual("Service resource not found", e.Message);
            }
        }

        [Test]
        public void GetEntityShouldGetToCorrectEndPoint()
        {
            var restResponseContent = new UsergridGetResponse<Friend> {Entities = new List<Friend>(), Cursor = ""};
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string identifer = "identifier";

            _entityManager.GetEntity<Friend>(collectionName, identifer);

            _request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}/{1}", collectionName, identifer)),
                Arg.Is(HttpMethod.Get),
                Arg.Any<object>());
        }

        [Test]
        public async void GetEntityShouldReturnEntityCorrectly()
        {
            var friend = new Friend {Name = "name", Age = 1};

            const string restResponseContent = "{\"entities\": [" +
                                               "{" +
                                               "\"uuid\": \"bcb343ba-d6d1-11e2-a295-7b4b45081d3b\"," +
                                               "\"type\": \"friend\"," +
                                               "\"name\": \"name\"," +
                                               "\"age\": 1," +
                                               "\"created\": 1371420707691," +
                                               "\"modified\": 1371420707691," +
                                               "\"metadata\": {" +
                                               "  \"path\": \"/friends/bcb343ba-d6d1-11e2-a295-7b4b45081d3b\"" +
                                               "}," +
                                               "}" +
                                               "]}";
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string identifier = "identifier";

            Friend returnedFriend = await _entityManager.GetEntity<Friend>(collectionName, identifier);

            Assert.IsNotNull(returnedFriend);
            Assert.AreEqual(friend.Name, returnedFriend.Name);
            Assert.AreEqual(friend.Age, returnedFriend.Age);
        }


        [Test]
        public async void GetEntityShouldReturnFirstEntityInListCorrectly()
        {
            var friend1 = new Friend {Name = "name1", Age = 1};
            var friend2 = new Friend {Name = "name2", Age = 2};

            var entities = new List<Friend> {friend1, friend2};
            var restResponseContent = new UsergridGetResponse<Friend> {Entities = entities, Cursor = "cursor"};
            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";
            const string identifier = "identifier";

            Friend returnedFriend = await _entityManager.GetEntity<Friend>(collectionName, identifier);

            Assert.IsNotNull(returnedFriend);
            Assert.AreEqual(friend1.Name, returnedFriend.Name);
            Assert.AreEqual(friend1.Age, returnedFriend.Age);
        }


        [Test]
        public void UpdateEntityShouldPutToCorrectEndPoint()
        {
            const string collectionName = "collection";
            const string identifier = "identifier";
            var entityToPost = new {FirstName = "first", LastName = "last"};

            IRestResponse restResponse = Helpers.SetUpRestResponse(HttpStatusCode.OK);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            _entityManager.UpdateEntity(collectionName, identifier, entityToPost);

            _request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}/{1}", collectionName, identifier)),
                Arg.Is(HttpMethod.Put),
                Arg.Is(entityToPost));
        }

        [Test]
        public async void UpdateEntityShouldTranslateToUserGridErrorAndThrowWhenServiceReturnsNotFound()
        {
            var restResponseContent = new UsergridError
                                          {
                                              Description = "Service resource not found",
                                              Error = "service_resource_not_found"
                                          };

            const string collectionName = "collection";
            const string identifier = "identifier";
            var entityToPost = new {FirstName = "first", LastName = "last"};

            IRestResponse restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.NotFound, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            try
            {
                await _entityManager.UpdateEntity(collectionName, identifier, entityToPost);
                new AssertionException("UserGridException was expected to be thrown here");
            }
            catch (UsergridException e)
            {
                Assert.AreEqual("service_resource_not_found", e.ErrorCode);
                Assert.AreEqual("Service resource not found", e.Message);
            }
        }

        [Test]
        public void GetEntitiesShouldGetToCorrectEndPointWithDefaultLimitAndQuery()
        {
            var restResponseContent = new UsergridGetResponse<Friend> { Entities = new List<Friend>(), Cursor = "" };
            var restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            const string collectionName = "collection";

            _entityManager.GetEntities<Friend>(collectionName);

            _request.Received(1).ExecuteJsonRequest(
                Arg.Is(string.Format("/{0}?limit=10", collectionName)),
                Arg.Is(HttpMethod.Get),
                Arg.Is<object>(x=> x == null));
        }

        [Test]
        public async void GetEntitiesShouldReturnListCorrectly()
        {
            var friend1 = new Friend { Name = "name1", Age = 1 };
            var friend2 = new Friend { Name = "name2", Age = 2 };
            var entities = new List<Friend> { friend1, friend2 };
            var restResponseContent = new UsergridGetResponse<Friend> { Entities = entities, Cursor = "cursor" };
            var restResponse = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent);

            _request
                .ExecuteJsonRequest(Arg.Any<string>(), Arg.Any<HttpMethod>(), Arg.Any<object>())
                .Returns(Task.FromResult(restResponse));

            var friends = await _entityManager.GetEntities<Friend>("collection");

            Assert.IsNotNull(friends);
            Assert.AreEqual(entities.Count, friends.Count);
            Assert.AreEqual(friend1.Name, friends[0].Name);
            Assert.AreEqual(friend1.Age, friends[0].Age);
            Assert.AreEqual(friend2.Name, friends[1].Name);
            Assert.AreEqual(friend2.Age, friends[1].Age);
        }

        [Test]
        public async void GetNextEntitiesShouldReturnEmptyListIfCalledBeforeGetEntities()
        {
            var friends = await _entityManager.GetNextEntities<Friend>("collection");

            CollectionAssert.IsEmpty(friends);
        }

        [Test]
        public async void GetNextEntitiesShouldGetToCorrectEndPointWithCorrectCursorState()
        {
            var friend1 = new Friend { Name = "name1", Age = 1 };
            var friend2 = new Friend { Name = "name2", Age = 2 };
            var friend3 = new Friend { Name = "name3", Age = 3 };
            var friend4 = new Friend { Name = "name4", Age = 4 };
            var friend5 = new Friend { Name = "name5", Age = 5 };
            var friend6 = new Friend { Name = "name6", Age = 6 };
            var entities1 = new List<Friend> { friend1, friend2 };
            var entities2 = new List<Friend> { friend3, friend4 };
            var entities3 = new List<Friend> { friend5, friend6 };
            var restResponseContent1 = new UsergridGetResponse<Friend> { Entities = entities1, Cursor = "cursor1" };
            var restResponse1 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent1);
            var restResponseContent2 = new UsergridGetResponse<Friend> { Entities = entities2, Cursor = "cursor2" };
            var restResponse2 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent2);
            var restResponseContent3 = new UsergridGetResponse<Friend> { Entities = entities3 };
            var restResponse3 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent3);

            _request
                .ExecuteJsonRequest("/collection?limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse1));
            _request
                .ExecuteJsonRequest("/collection?cursor=cursor1&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse2));
            _request
                .ExecuteJsonRequest("/collection?cursor=cursor2&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse3));

            UsergridCollection<Friend> list1 = await _entityManager.GetEntities<Friend>("collection");
            UsergridCollection<Friend> list2 = await _entityManager.GetNextEntities<Friend>("collection");
            UsergridCollection<Friend> list3 = await _entityManager.GetNextEntities<Friend>("collection");

            Assert.AreEqual(entities1[0].Name,list1[0].Name);
            Assert.AreEqual(entities1[1].Age,list1[1].Age);
            Assert.IsTrue(list1.HasNext);
            Assert.IsFalse(list1.HasPrevious);
            Assert.AreEqual(entities2[0].Name,list2[0].Name);
            Assert.AreEqual(entities2[1].Age,list2[1].Age);
            Assert.IsTrue(list2.HasNext);
            Assert.IsTrue(list2.HasPrevious);
            Assert.AreEqual(entities3[0].Name,list3[0].Name);
            Assert.AreEqual(entities3[1].Age,list3[1].Age);
            Assert.IsFalse(list3.HasNext);
            Assert.IsTrue(list3.HasPrevious);
        }

        [Test]
        public async void GetPreviousEntitiesShouldGetToCorrectEndPointWithCorrectCursorState()
        {
            var friend1 = new Friend { Name = "name1", Age = 1 };
            var friend2 = new Friend { Name = "name2", Age = 2 };
            var friend3 = new Friend { Name = "name3", Age = 3 };
            var friend4 = new Friend { Name = "name4", Age = 4 };
            var friend5 = new Friend { Name = "name5", Age = 5 };
            var friend6 = new Friend { Name = "name6", Age = 6 };
            var entities1 = new List<Friend> { friend1, friend2 };
            var entities2 = new List<Friend> { friend3, friend4 };
            var entities3 = new List<Friend> { friend5, friend6 };
            var restResponseContent1 = new UsergridGetResponse<Friend> { Entities = entities1, Cursor = "cursor1" };
            var restResponse1 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent1);
            var restResponseContent2 = new UsergridGetResponse<Friend> { Entities = entities2, Cursor = "cursor2" };
            var restResponse2 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent2);
            var restResponseContent3 = new UsergridGetResponse<Friend> { Entities = entities3 };
            var restResponse3 = Helpers.SetUpRestResponseWithContent(HttpStatusCode.OK, restResponseContent3);

            _request
               .ExecuteJsonRequest("/collection?limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
               .Returns(Task.FromResult(restResponse1));
            _request
                .ExecuteJsonRequest("/collection?cursor=cursor1&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse2));
            _request
                .ExecuteJsonRequest("/collection?cursor=cursor2&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse3));
            _request
                .ExecuteJsonRequest("/collection?cursor=cursor1&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse2));
            _request
                .ExecuteJsonRequest("/collection?&limit=10", HttpMethod.Get, Arg.Is<object>(x => x == null))
                .Returns(Task.FromResult(restResponse1));

            UsergridCollection<Friend> list1 = await _entityManager.GetEntities<Friend>("collection");
            UsergridCollection<Friend> list2 = await _entityManager.GetNextEntities<Friend>("collection");
            UsergridCollection<Friend> list3 = await _entityManager.GetNextEntities<Friend>("collection");
            UsergridCollection<Friend> list4 = await _entityManager.GetPreviousEntities<Friend>("collection");
            UsergridCollection<Friend> list5 = await _entityManager.GetPreviousEntities<Friend>("collection");

            Assert.AreEqual(entities1[0].Name,list1[0].Name);
            Assert.AreEqual(entities1[1].Age,list1[1].Age);
            Assert.IsTrue(list1.HasNext);
            Assert.IsFalse(list1.HasPrevious);

            Assert.AreEqual(entities2[0].Name,list2[0].Name);
            Assert.AreEqual(entities2[1].Age,list2[1].Age);
            Assert.IsTrue(list2.HasNext);
            Assert.IsTrue(list2.HasPrevious);
            
            Assert.AreEqual(entities3[0].Name,list3[0].Name);
            Assert.AreEqual(entities3[1].Age,list3[1].Age);
            Assert.IsFalse(list3.HasNext);
            Assert.IsTrue(list3.HasPrevious);

            Assert.AreEqual(entities2[0].Name, list4[0].Name);
            Assert.AreEqual(entities2[1].Age, list4[1].Age);
            Assert.IsTrue(list4.HasNext);
            Assert.IsTrue(list4.HasPrevious);

            Assert.AreEqual(entities1[0].Name, list5[0].Name);
            Assert.AreEqual(entities1[1].Age, list5[1].Age);
            Assert.IsTrue(list5.HasNext);
            Assert.IsFalse(list5.HasPrevious);
        }
    }
}