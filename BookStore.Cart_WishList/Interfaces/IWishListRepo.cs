using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;

namespace BookStore.Cart_WishList.Interfaces
{
    public interface IWishListRepo
    {
        Task<WishList> GetWishListByUserIdAsync(int userId);
        Task SaveWishListAsync(WishList wishList);
        
    }
}
