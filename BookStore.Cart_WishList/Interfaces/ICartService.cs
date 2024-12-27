using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;

namespace BookStore.Cart_WishList.Interfaces
{
    public interface ICartService
    {
        Task<Apiresponse<Cart>> AddToCartAsync(AddToCartRequestDto addToCartRequestDto);
        Task<Apiresponse<Cart>> UpdateCartAsync(AddToCartRequestDto addToCartRequestDto);
        Task<Apiresponse<CartDetailsDto>> GetAllItemsFromCartAsync();
        Task<Apiresponse<Cart>> DeleteFromCartAsync(int bookdId);

    }
}
