using BookStore.Cart_WishList.Data;
using BookStore.Cart_WishList.Repositories;
using BookStore.Cart_WishList.Model.Entities;
using Mongo2Go;
using Moq;
using NUnit.Framework;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace BookStore.Cart.Tests
{
    [TestFixture]
    public class CartRepoTests : IDisposable
    {
        private MongoDbService _mockMongoDbService;
        private CartRepo _cartRepo;
        private MongoDbRunner _mongoDbRunner;
        private IMongoCollection<BookStore.Cart_WishList.Model.Entities.Cart> _mockCollection;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _mongoDbRunner = MongoDbRunner.Start();
            _connectionString = $"{_mongoDbRunner.ConnectionString}TestDatabase";
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["ConnectionStrings:DbConnection"]).Returns(_connectionString);
            _mockMongoDbService = new MongoDbService(configurationMock.Object);
            _mockCollection = _mockMongoDbService.Database.GetCollection<BookStore.Cart_WishList.Model.Entities.Cart>("cart");
            _cartRepo = new CartRepo(_mockMongoDbService);
        }

        [Test]
        public async Task SaveCartAsync_ShouldInsertNewCart_WhenUserIdDoesNotExist()
        {
            var cart = new BookStore.Cart_WishList.Model.Entities.Cart
            {
                UserId = 2,
                Books = new List<CartBook> { new CartBook { BookId = 102, Quantity = 2 } },
                TotalPrice = 40,
                TotalQuantity = 2
            };

            await _cartRepo.SaveCartAsync(cart);

            var savedCart = await _mockCollection.Find(c => c.UserId == 2).FirstOrDefaultAsync();

            Assert.IsNotNull(savedCart);
            Assert.AreEqual(2, savedCart.UserId);
            Assert.AreEqual(1, savedCart.Books.Count);
            Assert.AreEqual(102, savedCart.Books[0].BookId);
            Assert.AreEqual(2, savedCart.Books[0].Quantity);
            Assert.AreEqual(2, savedCart.TotalQuantity);
        }

        [Test]
        public async Task GetCartByUserIdAsync_ShouldReturnCart_WhenUserIdExists()
        {
            var cart = new BookStore.Cart_WishList.Model.Entities.Cart
            {
                UserId = 2,
                Books = new List<CartBook>
                {
                    new CartBook { BookId = 101, Quantity = 1 },
                    new CartBook { BookId = 102, Quantity = 3 }
                },
                TotalPrice = 100,
                TotalQuantity = 4
            };

            await _mockCollection.InsertOneAsync(cart);

            var retrievedCart = await _cartRepo.GetCartByUserIdAsync(2);

            Assert.IsNotNull(retrievedCart);
            Assert.AreEqual(2, retrievedCart.UserId);
            Assert.AreEqual(2, retrievedCart.Books.Count);
            Assert.AreEqual(101, retrievedCart.Books[0].BookId);
            Assert.AreEqual(102, retrievedCart.Books[1].BookId);
            Assert.AreEqual(1, retrievedCart.Books[0].Quantity);
            Assert.AreEqual(3, retrievedCart.Books[1].Quantity);
            Assert.AreEqual(100, retrievedCart.TotalPrice);
            Assert.AreEqual(4, retrievedCart.TotalQuantity);
        }

        [Test]
        public async Task GetCartByUserIdAsync_ShouldReturnNull_WhenUserIdDoesNotExist()
        {
            var retrievedCart = await _cartRepo.GetCartByUserIdAsync(999);
            Assert.IsNull(retrievedCart);
        }

        [Test]
        public async Task DeleteCartAsync_ShouldDeleteCart_WhenCartIdExists()
        {
            var cart = new BookStore.Cart_WishList.Model.Entities.Cart
            {
                Id = ObjectId.GenerateNewId(),
                UserId = 2,
                Books = new List<CartBook>
                {
                    new CartBook { BookId = 101, Quantity = 1 }
                },
                TotalPrice = 50,
                TotalQuantity = 1
            };

            await _mockCollection.InsertOneAsync(cart);

            await _cartRepo.DeleteCartAsync(cart.Id.ToString());

            var deletedCart = await _mockCollection.Find(c => c.Id == cart.Id).FirstOrDefaultAsync();
            Assert.IsNull(deletedCart);
        }

        [Test]
        public void DeleteCartAsync_ShouldThrowException_WhenCartIdDoesNotExist()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();
            var exception = Assert.ThrowsAsync<Exception>(async () => await _cartRepo.DeleteCartAsync(nonExistentId));
            Assert.AreEqual($"No cart found with ID {nonExistentId}.", exception.Message);
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
