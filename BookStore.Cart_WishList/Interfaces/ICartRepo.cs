using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;

namespace BookStore.Cart_WishList.Interfaces
{
    public interface ICartRepo
    {
        Task<Cart> GetCartByUserIdAsync(int userId);
        Task SaveCartAsync(Cart cart);
        Task DeleteCartAsync(string cartId);
    }
}
