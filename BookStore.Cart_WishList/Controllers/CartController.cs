using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Cart_WishList.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController:ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(AddToCartRequestDto addToCartRequestDto)
        {
            var result=await _cartService.AddToCartAsync(addToCartRequestDto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateCart(AddToCartRequestDto addToCartRequestDto)
        {
            var result = await _cartService.UpdateCartAsync(addToCartRequestDto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetItemsFromCart()
        {
            Console.WriteLine("Called fetch items");
            Console.WriteLine($"Authorization Header: {Request.Headers["Authorization"]}");
            var result = await _cartService.GetAllItemsFromCartAsync();
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpDelete("{bookId}")]
        [Authorize]
        public async Task<IActionResult> DeleteFromCart(int bookId)
        {
            var result = await _cartService.DeleteFromCartAsync(bookId);
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
