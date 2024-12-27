using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Cart_WishList.Controllers
{
    [ApiController]
    [Route("api/wishlist")]
    public class WishListController:ControllerBase
    {
        private readonly IWishListService _wishListService;
        public WishListController(IWishListService wishListService)
        {
            _wishListService = wishListService;
        }
        [Authorize]
        [HttpPost]
       
        public async Task<IActionResult> AddToWishList(AddToWishListDto addToWishListDto)
        {
            var result=await _wishListService.AddToWishListAsync(addToWishListDto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetItemsFromWishList()
        {
            var result = await _wishListService.GetItemsFromWishListAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [Authorize]
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteItemFromWishList(int bookId)
        {
            var result = await _wishListService.DeleteFromWishListAsync(bookId);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
