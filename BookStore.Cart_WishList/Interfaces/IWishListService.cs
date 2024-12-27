using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;

namespace BookStore.Cart_WishList.Interfaces
{
    public interface IWishListService
    {
        Task<Apiresponse<WishList>> AddToWishListAsync(AddToWishListDto requestDto);
        Task<Apiresponse<List<WishListItemDto>>> GetItemsFromWishListAsync();
        Task<Apiresponse<string>> DeleteFromWishListAsync(int bookId);
    }
}
