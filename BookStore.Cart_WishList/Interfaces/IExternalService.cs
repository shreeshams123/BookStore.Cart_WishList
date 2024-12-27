using BookStore.Cart_WishList.Model.DTOs;

namespace BookStore.Cart_WishList.Interfaces
{
    public interface IExternalService
    {
        Task<BookDataDto> BookExists(int bookId);
    }
}
