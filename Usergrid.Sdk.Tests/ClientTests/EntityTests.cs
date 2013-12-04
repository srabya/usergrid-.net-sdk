﻿using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Usergrid.Sdk.Manager;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.Tests.ClientTests
{
    [TestFixture]
    public class EntityTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            _entityManager = Substitute.For<IEntityManager>();
            _client = new Client(null, null) {EntityManager = _entityManager};
        }

        #endregion

        private IEntityManager _entityManager;
        private IClient _client;


        [Test]
        public void CreateEntityShouldDelegateToEntityManagerWithCorrectParameters()
        {
            var entity = new {Name = "test"};

            _client.CreateEntity<object>("collection", entity);

            _entityManager.Received(1).CreateEntity<object>("collection", entity);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void CreateEntityShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.CreateEntity(Arg.Any<string>(), Arg.Any<object>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.CreateEntity<object>(null, null);
        }

        [Test]
        public async void CreateEntityShouldReturnUserGridEntity()
        {
            var entity = new object();
            _entityManager.CreateEntity("collection", entity).ReturnsForAnyArgs(Task.FromResult(entity));

            var returnedEntity = await _client.CreateEntity("collection", entity);

            Assert.AreEqual(entity, returnedEntity);
        }

        [Test]
        public void DeleteEntityShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.DeleteEntity("collection", "identifier");

            _entityManager.Received(1).DeleteEntity("collection", "identifier");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void DeleteEntityShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.DeleteEntity(Arg.Any<string>(), Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.DeleteEntity(null, null);
        }

        [Test]
        public void GetEntitiesShouldDefaultTheLimit()
        {
            _client.GetEntities<object>("collection");

            _entityManager.Received(1).GetEntities<object>("collection", 10);
        }

        [Test]
        public void GetEntitiesShouldDefaultTheQueryToNull()
        {
            _client.GetEntities<object>("collection", 10);

            _entityManager.Received(1).GetEntities<object>("collection", 10, null);
        }

        [Test]
        public void GetEntitiesShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.GetEntities<object>("collection", 20, "query");

            _entityManager.Received(1).GetEntities<object>("collection", 20, "query");
        }

        [Test]
        public void GetEntityShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.GetEntity<object>("collection", "identifier");

            _entityManager.Received(1).GetEntity<object>("collection", "identifier");
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void GetEntityShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.GetEntity<object>(Arg.Any<string>(), Arg.Any<string>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.GetEntity<object>(null, null);
        }

        [Test]
        public async void GetEntityShouldReturnEntityFromEntityManager()
        {
            var entity = new object();
            _entityManager.GetEntity<object>("collection", "identifier").ReturnsForAnyArgs(Task.FromResult(entity));

            object createdEntity = await _client.GetEntity<object>("collection", "identifier");

            Assert.AreEqual(entity, createdEntity);
        }

        [Test]
        public async void GetEntityShouldReturnNullForUnexistingEntity() {
            UsergridEntity entity = null;
            _entityManager.GetEntity<UsergridEntity>("collection", "identifier").Returns(x => Task.FromResult(entity));

            var usergridEntity = await _client.GetEntity<UsergridDevice>("collection", "identifier");

            Assert.IsNull(usergridEntity);
        }

        [Test]
        public void GetNextEntitiesShouldDefaultTheQueryToNull()
        {
            _client.GetNextEntities<object>("collection");

            _entityManager.Received(1).GetNextEntities<object>("collection", null);
        }

        [Test]
        public void GetNextEntitiesShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.GetNextEntities<object>("collection", "query");

            _entityManager.Received(1).GetNextEntities<object>("collection", "query");
        }

        [Test]
        public void GetPreviousEntitiesShouldDefaultTheQueryToNull()
        {
            _client.GetPreviousEntities<object>("collection");

            _entityManager.Received(1).GetPreviousEntities<object>("collection", null);
        }

        [Test]
        public void GetPreviousEntitiesShouldDelegateToEntityManagerWithCorrectParameters()
        {
            _client.GetPreviousEntities<object>("collection", "query");

            _entityManager.Received(1).GetPreviousEntities<object>("collection", "query");
        }

        [Test]
        public void UpdateEntityShouldDelegateToEntityManagerWithCorrectParameters()
        {
            var entity = new {Name = "test"};

            _client.UpdateEntity<object>("collection", "identifier", entity);

            _entityManager.Received(1).UpdateEntity<object>("collection", "identifier", entity);
        }

        [Test]
        [ExpectedException(ExpectedException = typeof (UsergridException), ExpectedMessage = "Exception message")]
        public async void UpdateEntityShouldPassOnTheException()
        {
            _entityManager
                .When(m => m.UpdateEntity(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<object>()))
                .Do(m => { throw new UsergridException(new UsergridError {Description = "Exception message"}); });

            await _client.UpdateEntity<object>(null, null, null);
        }
    }
}