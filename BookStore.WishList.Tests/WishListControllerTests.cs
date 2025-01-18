using BookStore.Cart_WishList.Controllers;
using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BookStore.WishList.Tests
{
    [TestFixture]
    public class WishListControllerTests
    {
        private Mock<IWishListService> _mockWishListService;
        private WishListController _wishListController;

        [SetUp]
        public void SetUp()
        {
            _mockWishListService = new Mock<IWishListService>();
            _wishListController = new WishListController(_mockWishListService.Object);
        }

        [Test]
        public async Task AddToWishList_ReturnsOk_WhenAddToWishListIsSuccessful()
        {
            var addToWishListRequestDto = new AddToWishListDto { BookId = 1 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.WishList>
            {
                Success = true,
                Message = "Book added to wishlist successfully",
                Data = new BookStore.Cart_WishList.Model.Entities.WishList()
            };

            _mockWishListService.Setup(service => service.AddToWishListAsync(It.IsAny<AddToWishListDto>()))
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.AddToWishList(addToWishListRequestDto);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task AddToWishList_ReturnsBadRequest_WhenAddToWishListFails()
        {
            var addToWishListRequestDto = new AddToWishListDto { BookId = 1 };
            var apiResponse = new Apiresponse<BookStore.Cart_WishList.Model.Entities.WishList>
            {
                Success = false,
                Message = "Failed to add to wishlist"
            };

            _mockWishListService.Setup(service => service.AddToWishListAsync(It.IsAny<AddToWishListDto>()))
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.AddToWishList(addToWishListRequestDto);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }

        [Test]
        public async Task GetItemsFromWishList_ReturnsOk_WhenItemsExistInWishList()
        {
            var apiResponse = new Apiresponse<List<WishListItemDto>>
            {
                Success = true,
                Message = "Retrieved wishlist items successfully",
                Data = new List<WishListItemDto>
                {
                    new WishListItemDto { BookId = 1, title = "Test Book", Author = "Test Author" }
                }
            };

            _mockWishListService.Setup(service => service.GetItemsFromWishListAsync())
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.GetItemsFromWishList();

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task GetItemsFromWishList_ReturnsNoContent_WhenNoItemsExistInWishList()
        {
            var apiResponse = new Apiresponse<List<WishListItemDto>>
            {
                Success = true,
                Message = "No items found in the wishlist",
                Data = new List<WishListItemDto>()
            };

            _mockWishListService.Setup(service => service.GetItemsFromWishListAsync())
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.GetItemsFromWishList();

            var noContentResult = result as OkObjectResult;
            Assert.AreEqual(200, noContentResult.StatusCode);
        }

        [Test]
        public async Task DeleteItemFromWishList_ReturnsOk_WhenDeleteFromWishListIsSuccessful()
        {
            var bookId = 1;
            var apiResponse = new Apiresponse<string>
            {
                Success = true,
                Message = "Book removed from wishlist successfully"
            };

            _mockWishListService.Setup(service => service.DeleteFromWishListAsync(bookId))
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.DeleteItemFromWishList(bookId);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(apiResponse, okResult.Value);
        }

        [Test]
        public async Task DeleteItemFromWishList_ReturnsBadRequest_WhenDeleteFromWishListFails()
        {
            var bookId = 1;
            var apiResponse = new Apiresponse<string>
            {
                Success = false,
                Message = "Failed to remove book from wishlist"
            };

            _mockWishListService.Setup(service => service.DeleteFromWishListAsync(bookId))
                .ReturnsAsync(apiResponse);

            var result = await _wishListController.DeleteItemFromWishList(bookId);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual(apiResponse, badRequestResult.Value);
        }
    }
}
