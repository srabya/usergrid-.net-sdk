using System;
using NUnit.Framework;
using Usergrid.Sdk.Model;

namespace Usergrid.Sdk.IntegrationTests {
    public class Page {
        public int PageNumber { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    public class EntityPagingTests : BaseTest {
        [Test]
        public async void ShouldDoPaging() {
            var client = new Client(Organization, Application);
            await client.Login(ClientId, ClientSecret, AuthType.Organization);

            string pagingCollectionName = "paging";
            for (int i = 0; i < 20; i++) {
                try {
                    await client.DeleteEntity(pagingCollectionName, "page-" + i);
                    Console.WriteLine("Deleted " + i);
                }
                catch (UsergridException) {}
            }

            for (int i = 0; i < 20; i++) {
                var page = new Page {PageNumber = i, Name = "page-" + i};
                await client.CreateEntity(pagingCollectionName, page);
                Console.WriteLine("Created " + i);
            }


            var collection = await client.GetEntities<Page>(pagingCollectionName, 3);
            Assert.AreEqual(3, collection.Count);
            Assert.IsTrue(collection.HasNext);
            Assert.IsFalse(collection.HasPrevious);
            Assert.AreEqual("page-1", collection[1].Name);

            collection = await client.GetNextEntities<Page>(pagingCollectionName);
            Assert.IsTrue(collection.HasNext);
            Assert.IsTrue(collection.HasPrevious);
            Assert.AreEqual("page-4", collection[1].Name);

            collection = await client.GetNextEntities<Page>(pagingCollectionName);
            Assert.IsTrue(collection.HasNext);
            Assert.IsTrue(collection.HasPrevious);
            Assert.AreEqual("page-7", collection[1].Name);

            collection = await client.GetPreviousEntities<Page>(pagingCollectionName);
            Assert.IsTrue(collection.HasNext);
            Assert.IsTrue(collection.HasPrevious);
            Assert.AreEqual("page-4", collection[1].Name);

            for (int i = 0; i < 20; i++) {
                await client.DeleteEntity(pagingCollectionName, "page-" + i);
                Console.WriteLine("Deleted " + i);
            }
        }
    }
}