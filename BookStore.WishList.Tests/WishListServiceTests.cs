using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore.WishList.Tests
{
    [TestFixture]
    public class WishListServiceTests
    {
        private Mock<IWishListRepo> _mockWishListRepo;
        private Mock<TokenService> _mockTokenService;
        private Mock<IExternalService> _mockExternalService;
        private WishListService _wishListService;

        [SetUp]
        public void Setup()
        {
            _mockWishListRepo = new Mock<IWishListRepo>();
            _mockTokenService = new Mock<TokenService>(Mock.Of<IConfiguration>(), Mock.Of<IHttpContextAccessor>(), Mock.Of<ILogger<TokenService>>());
            _mockExternalService = new Mock<IExternalService>();

            _wishListService = new WishListService(
                _mockTokenService.Object,
                _mockExternalService.Object,
                _mockWishListRepo.Object
            );
        }

        [Test]
        public async Task AddToWishListAsync_BookDoesNotExist_ReturnsErrorResponse()
        {
            var requestDto = new AddToWishListDto { BookId = 1 };
            _mockExternalService.Setup(es => es.BookExists(requestDto.BookId)).ReturnsAsync((BookDataDto)null);

            var result = await _wishListService.AddToWishListAsync(requestDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Book with ID {requestDto.BookId} does not exist.", result.Message);
        }

        [Test]
        public async Task AddToWishListAsync_BookAlreadyInWishList_ReturnsErrorResponse()
        {
            var requestDto = new AddToWishListDto { BookId = 1 };
            var userId = 123;
            var existingWishList = new BookStore.Cart_WishList.Model.Entities.WishList { UserId = userId, Books = new List<WishListBook> { new WishListBook { BookId = 1 } } };

            _mockExternalService.Setup(es => es.BookExists(requestDto.BookId)).ReturnsAsync(new BookDataDto { BookId = 1 });
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync(existingWishList);

            var result = await _wishListService.AddToWishListAsync(requestDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Book is already in the wishlist.", result.Message);
        }

        [Test]
        public async Task AddToWishListAsync_ValidRequest_AddsBookToWishList()
        {
            var requestDto = new AddToWishListDto { BookId = 1 };
            var userId = 123;
            var book = new BookDataDto { BookId = 1 };
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList { UserId = userId };

            _mockExternalService.Setup(es => es.BookExists(requestDto.BookId)).ReturnsAsync(book);
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync((BookStore.Cart_WishList.Model.Entities.WishList)null);
            _mockWishListRepo.Setup(wl => wl.SaveWishListAsync(It.IsAny<BookStore.Cart_WishList.Model.Entities.WishList>())).Returns(Task.CompletedTask);

            var result = await _wishListService.AddToWishListAsync(requestDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Book added to wishlist successfully.", result.Message);
            Assert.AreEqual(1, result.Data.Books.Count);
            Assert.AreEqual(requestDto.BookId, result.Data.Books.First().BookId);
        }

        [Test]
        public async Task GetItemsFromWishListAsync_NoItemsInWishList_ReturnsEmptyResponse()
        {
            var userId = 123;
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync((BookStore.Cart_WishList.Model.Entities.WishList)null);

            var result = await _wishListService.GetItemsFromWishListAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual("No items found in the wishlist.", result.Message);
            Assert.AreEqual(0, result.Data.Count);
        }

        [Test]
        public async Task GetItemsFromWishListAsync_ItemsExistInWishList_ReturnsWishListDetails()
        {
            var userId = 123;
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = userId,
                Books = new List<WishListBook>
                {
                    new WishListBook { BookId = 1 }
                }
            };
            var book = new BookDataDto { BookId = 1, Title = "Test Book", Price = 100 };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync(wishList);
            _mockExternalService.Setup(es => es.BookExists(1)).ReturnsAsync(book);

            var result = await _wishListService.GetItemsFromWishListAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Data.Count);
            Assert.AreEqual("Test Book", result.Data.First().title);
            Assert.AreEqual(100, result.Data.First().Price);
        }

        [Test]
        public async Task DeleteFromWishListAsync_BookNotInWishList_ReturnsErrorResponse()
        {
            var bookId = 1;
            var userId = 123;
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList { UserId = userId, Books = new List<WishListBook>() };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync(wishList);

            var result = await _wishListService.DeleteFromWishListAsync(bookId);

            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Wishlist is empty or does not exist.", result.Message);
        }

        [Test]
        public async Task DeleteFromWishListAsync_ValidRequest_RemovesBookFromWishList()
        {
            var bookId = 1;
            var userId = 123;
            var wishList = new BookStore.Cart_WishList.Model.Entities.WishList
            {
                UserId = userId,
                Books = new List<WishListBook> { new WishListBook { BookId = bookId } }
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);
            _mockWishListRepo.Setup(wl => wl.GetWishListByUserIdAsync(userId)).ReturnsAsync(wishList);
            _mockWishListRepo.Setup(wl => wl.SaveWishListAsync(It.IsAny<BookStore.Cart_WishList.Model.Entities.WishList>())).Returns(Task.CompletedTask);

            var result = await _wishListService.DeleteFromWishListAsync(bookId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual($"Book with ID {bookId} was successfully removed from the wishlist.", result.Message);
            Assert.AreEqual(0, wishList.Books.Count);
        }
    }
}
