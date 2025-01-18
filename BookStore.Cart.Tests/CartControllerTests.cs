using BookStore.Cart_WishList.Controllers;
using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model;
using BookStore.Cart_WishList.Model.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookStore.Cart.Tests
{

    [TestFixture]
    public class CartControllerTests
    {
        private Mock<ICartService> _mockCartService;
        private CartController _cartController;

        [SetUp]
        public void SetUp()
        {
            _mockCartService = new Mock<ICartService>();
            _cartController = new CartController(_mockCartService.Object);
        }

        [Test]
        public async Task AddToCart_ReturnsOk_WhenAddToCartIsSuccessful()
        {
            var addToCartRequestDto = new AddToCartRequestDto { BookId = 1, Quantity = 2 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.Cart>
            {
                Success = true,
                Message = "Added to cart successfully",
                Data = new BookStore.Cart_WishList.Model.Entities.Cart()
            };

            _mockCartService.Setup(service => service.AddToCartAsync(It.IsAny<AddToCartRequestDto>()))
                .ReturnsAsync(apiResponse);

            var result = await _cartController.AddToCart(addToCartRequestDto);

            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task AddToCart_ReturnsBadRequest_WhenAddToCartFails()
        {
            var addToCartRequestDto = new AddToCartRequestDto { BookId = 1, Quantity = 2 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.Cart>
            {
                Success = false,
                Message = "Failed to add to cart"
            };

            _mockCartService.Setup(service => service.AddToCartAsync(It.IsAny<AddToCartRequestDto>()))
                .ReturnsAsync(apiResponse);

            var result = await _cartController.AddToCart(addToCartRequestDto);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }

        [Test]
        public async Task UpdateCart_ReturnsOk_WhenUpdateCartIsSuccessful()
        {
            var addToCartRequestDto = new AddToCartRequestDto { BookId = 1, Quantity = 3 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.Cart> { Success = true, Message = "Cart updated successfully" };

            _mockCartService.Setup(service => service.UpdateCartAsync(addToCartRequestDto))
                .ReturnsAsync(apiResponse);

            var result = await _cartController.UpdateCart(addToCartRequestDto);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task UpdateCart_ReturnsBadRequest_WhenUpdateCartFails()
        {
            var addToCartRequestDto = new AddToCartRequestDto { BookId = 1, Quantity = 3 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.Cart> { Success = false, Message = "Failed to update cart" };

            _mockCartService.Setup(service => service.UpdateCartAsync(addToCartRequestDto))
                .ReturnsAsync(apiResponse);

            var result = await _cartController.UpdateCart(addToCartRequestDto);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }

        [Test]
        public async Task DeleteFromCart_ReturnsBadRequest_WhenItemDeletionFails()
        {
            var bookId = 1;
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.Cart> { Success = false, Message = "Failed to delete item from cart" };

            _mockCartService.Setup(service => service.DeleteFromCartAsync(bookId))
                .ReturnsAsync(apiResponse);

            var result = await _cartController.DeleteFromCart(bookId);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }
    }
}
