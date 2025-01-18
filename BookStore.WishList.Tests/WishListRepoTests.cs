using BookStore.Cart_WishList.Data;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Repositories;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Driver;
using Moq;

namespace BookStore.WishList.Tests
{
    [TestFixture]
    public class WishListRepoTests : IDisposable
    {
        private MongoDbService _mockMongoDbService;
        private WishListRepo _wishListRepo;
        private MongoDbRunner _mongoDbRunner;
        private IMongoCollection<BookStore.Cart_WishList.Model.Entities.WishList> _mockCollection;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            _connectionString = $"{_mongoDbRunner.ConnectionString}TestDatabase";
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["ConnectionStrings:DbConnection"]).Returns(_connectionString);
            _mockMongoDbService = new MongoDbService(configurationMock.Object);
            _mockCollection = _mockMongoDbService.Database.GetCollection<BookStore.Cart_WishList.Model.Entities.WishList>("wishlist");
            _wishListRepo = new WishListRepo(_mockMongoDbService);
        }

        [Test]
        public async Task SaveWishListAsync_ShouldInsertNewWishList_WhenUserIdDoesNotExist()
        {
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = 2,
                Books = new List<WishListBook> { new WishListBook { BookId = 102 } }
            };

            await _wishListRepo.SaveWishListAsync(wishList);

            var savedWishList = await _mockCollection.Find(w => w.UserId == 2).FirstOrDefaultAsync();

            Assert.IsNotNull(savedWishList);
            Assert.AreEqual(2, savedWishList.UserId);
            Assert.AreEqual(1, savedWishList.Books.Count);
            Assert.AreEqual(102, savedWishList.Books[0].BookId);
        }

        [Test]
        public async Task SaveWishListAsync_ShouldUpdateWishList_WhenUserIdExists()
        {
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = 2,
                Books = new List<WishListBook>
        {
            new WishListBook { BookId = 101},
            new WishListBook { BookId = 103}
        }
            };

            await _mockCollection.InsertOneAsync(wishList);

            var updatedWishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = 2,  
                Books = new List<WishListBook>
        {
            new WishListBook { BookId = 104}
        }
            };

            var filter = Builders<BookStore.Cart_WishList.Model.Entities.WishList>.Filter.Eq(w => w.UserId, updatedWishList.UserId);
            var update = Builders<BookStore.Cart_WishList.Model.Entities.WishList>.Update.Set(w => w.Books, updatedWishList.Books);

            await _mockCollection.UpdateOneAsync(filter, update);

            var savedWishList = await _mockCollection.Find(w => w.UserId == 2).FirstOrDefaultAsync();

            Assert.IsNotNull(savedWishList);
            Assert.AreEqual(2, savedWishList.UserId);
            Assert.AreEqual(1, savedWishList.Books.Count);
            Assert.AreEqual(104, savedWishList.Books[0].BookId);
        }


        [Test]
        public async Task GetWishListByUserIdAsync_ShouldReturnWishList_WhenUserIdExists()
        {
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = 2,
                Books = new List<WishListBook>
                {
                    new WishListBook { BookId = 101},
                    new WishListBook { BookId = 102 }
                }
            };

            await _mockCollection.InsertOneAsync(wishList);

            var retrievedWishList = await _wishListRepo.GetWishListByUserIdAsync(2);

            Assert.IsNotNull(retrievedWishList);
            Assert.AreEqual(2, retrievedWishList.UserId);
            Assert.AreEqual(2, retrievedWishList.Books.Count);
            Assert.AreEqual(101, retrievedWishList.Books[0].BookId);
            Assert.AreEqual(102, retrievedWishList.Books[1].BookId);

        }

        [Test]
        public async Task GetWishListByUserIdAsync_ShouldReturnNull_WhenUserIdDoesNotExist()
        {
            var retrievedWishList = await _wishListRepo.GetWishListByUserIdAsync(999);
            Assert.IsNull(retrievedWishList);
        }

        [TearDown]
        public void TearDown()
        {
            _mongoDbRunner?.Dispose();
        }

        public void Dispose()
        {
            TearDown();
        }

    }
}