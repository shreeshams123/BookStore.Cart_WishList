using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Cart.Tests
{
    [TestFixture]
    public class CartServiceTests
    {
        private Mock<ICartRepo> _mockCartRepo;
        private Mock<TokenService> _mockTokenService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<ILogger<TokenService>> _mockLogger;
        private Mock<IExternalService> _mockExternalService;
        private CartService _cartService;

        [SetUp]
        public void Setup()
        {
            _mockCartRepo = new Mock<ICartRepo>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<TokenService>>();
            _mockExternalService = new Mock<IExternalService>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
            var mockClaim = new Claim(ClaimTypes.NameIdentifier, "123");

            mockClaimsPrincipal.Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier)).Returns(mockClaim);
            mockHttpContext.Setup(ctx => ctx.User).Returns(mockClaimsPrincipal.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockTokenService = new Mock<TokenService>(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _mockLogger.Object
            );
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(123);

            _cartService = new CartService(
                _mockCartRepo.Object,
                _mockTokenService.Object,
                _mockExternalService.Object
            );
        }

        [Test]
        public async Task AddToCartAsync_BookDoesNotExist_ReturnsErrorResponse()
        {
            var requestDto = new AddToCartRequestDto { BookId = 1, Quantity = 2 };
            var userId = 123;

            _mockExternalService
                .Setup(es => es.BookExists(It.IsAny<int>()))
                .ReturnsAsync((BookDataDto)null);

            _mockTokenService
                .Setup(ts => ts.GetUserIdFromToken())
                .Returns(userId);

            var result = await _cartService.AddToCartAsync(requestDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Book with ID {requestDto.BookId} does not exist.", result.Message);
            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task AddToCartAsync_QuantityExceedsStock_ReturnsErrorResponse()
        {
            var requestDto = new AddToCartRequestDto { BookId = 1, Quantity = 10 };
            var book = new BookDataDto { BookId = 1, StockQuantity = 5, Price = 100 };
            var userId = 123;

            _mockExternalService.Setup(es => es.BookExists(It.IsAny<int>())).ReturnsAsync(book);
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            var result = await _cartService.AddToCartAsync(requestDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual($"Requested quantity ({requestDto.Quantity}) exceeds available stock ({book.StockQuantity}).", result.Message);
            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task AddToCartAsync_ValidRequest_AddsBookToCart()
        {
            var requestDto = new AddToCartRequestDto { BookId = 1, Quantity = 2 };
            var book = new BookDataDto { BookId = 1, StockQuantity = 5, Price = 100 };
            var userId = 123;

            _mockExternalService.Setup(es => es.BookExists(It.IsAny<int>())).ReturnsAsync(book);
            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCartRepo.Setup(cr => cr.GetCartByUserIdAsync(userId)).ReturnsAsync((BookStore.Cart_WishList.Model.Entities.Cart)null);

            var result = await _cartService.AddToCartAsync(requestDto);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Book added to cart successfully.", result.Message);
            Assert.AreEqual(1, result.Data.Books.Count);
            Assert.AreEqual(requestDto.BookId, result.Data.Books.First().BookId);
            Assert.AreEqual(requestDto.Quantity, result.Data.Books.First().Quantity);

            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task UpdateCartAsync_BookNotInCart_ReturnsErrorResponse()
        {
            var requestDto = new AddToCartRequestDto { BookId = 1, Quantity = 2 };
            var userId = 123;

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCartRepo.Setup(cr => cr.GetCartByUserIdAsync(userId))
                         .ReturnsAsync(new BookStore.Cart_WishList.Model.Entities.Cart { UserId = userId, Books = new List<CartBook>() });

            var result = await _cartService.UpdateCartAsync(requestDto);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Book with ID 1 does not exist.", result.Message);

            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task DeleteFromCartAsync_ValidRequest_RemovesBookFromCart()
        {
            var bookId = 1;
            var userId = 123;
            var cart = new BookStore.Cart_WishList.Model.Entities.Cart
            {
                UserId = userId,
                Books = new List<CartBook>
                {
                    new CartBook { BookId = bookId, Quantity = 2 }
                }
            };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCartRepo.Setup(cr => cr.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);
            _mockCartRepo.Setup(cr => cr.SaveCartAsync(It.IsAny<BookStore.Cart_WishList.Model.Entities.Cart>())).Returns(Task.CompletedTask);

            var result = await _cartService.DeleteFromCartAsync(bookId);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Cart is empty and has been deleted.", result.Message);
            Assert.AreEqual(0, cart.Books.Count);

            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task GetAllItemsFromCartAsync_NoItemsInCart_ReturnsEmptyResponse()
        {
            var userId = 1;

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCartRepo.Setup(cr => cr.GetCartByUserIdAsync(userId)).ReturnsAsync((BookStore.Cart_WishList.Model.Entities.Cart)null);

            var result = await _cartService.GetAllItemsFromCartAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual("No items found in the cart.", result.Message);
            Assert.AreEqual(0, result.Data.TotalQuantity);

            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);
        }

        [Test]
        public async Task GetAllItemsFromCartAsync_ItemsExistInCart_ReturnsCartDetails()
        {
            var userId = 1;
            var cart = new BookStore.Cart_WishList.Model.Entities.Cart
            {
                UserId = userId,
                Books = new List<CartBook>
                {
                    new CartBook { BookId = 1, Quantity = 2 }
                }
            };
            var book = new BookDataDto { BookId = 1, Title = "Test Book", Price = 100, StockQuantity = 5 };

            _mockTokenService.Setup(ts => ts.GetUserIdFromToken()).Returns(userId);

            _mockCartRepo.Setup(cr => cr.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);

            _mockExternalService.Setup(es => es.BookExists(1)).ReturnsAsync(book);

            var result = await _cartService.GetAllItemsFromCartAsync();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Data.CartItems.Count);
            Assert.AreEqual("Test Book", result.Data.CartItems.First().title);
            Assert.AreEqual(2, result.Data.CartItems.First().Quantity);

            _mockTokenService.Verify(ts => ts.GetUserIdFromToken(), Times.Once);

            _mockExternalService.Verify(es => es.BookExists(1), Times.Once);
        }
    }
}
