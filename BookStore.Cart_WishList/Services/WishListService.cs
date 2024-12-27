using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;
using BookStore.Cart_WishList.Repositories;

namespace BookStore.Cart_WishList.Services
{
    public class WishListService:IWishListService
    {
        private readonly TokenService _tokenService;
        private readonly IExternalService _externalService;
        private readonly IWishListRepo _wishListRepo;
        public WishListService(TokenService tokenService, IExternalService externalService, IWishListRepo wishListRepo)
        {
            _tokenService = tokenService;
            _externalService = externalService;
            _wishListRepo = wishListRepo;
        }
        public async Task<Apiresponse<WishList>> AddToWishListAsync(AddToWishListDto requestDto)
        {
            var book = await _externalService.BookExists(requestDto.BookId);
            if (book == null)
            {
                return new Apiresponse<WishList>
                {
                    Success = false,
                    Message = $"Book with ID {requestDto.BookId} does not exist."
                };
            }

            int userId = _tokenService.GetUserIdFromToken();
            var wishList = await _wishListRepo.GetWishListByUserIdAsync(userId)
                          ?? new WishList { UserId = userId };

            if (wishList.Books.Any(b => b.BookId == requestDto.BookId))
            {
                return new Apiresponse<WishList>
                {
                    Success = false,
                    Message = "Book is already in the wishlist."
                };
            }

            wishList.Books.Add(new WishListBook
            {
                BookId = requestDto.BookId
            });

            await _wishListRepo.SaveWishListAsync(wishList);

            return new Apiresponse<WishList>
            {
                Success = true,
                Message = "Book added to wishlist successfully.",
                Data = wishList
            };
        }
        public async Task<Apiresponse<List<WishListItemDto>>> GetItemsFromWishListAsync()
        {
            int userId = _tokenService.GetUserIdFromToken();

            var wishList = await _wishListRepo.GetWishListByUserIdAsync(userId);
            if (wishList == null || !wishList.Books.Any())
            {
                return new Apiresponse<List<WishListItemDto>>
                {
                    Success = false,
                    Message = "No items found in the wishlist."
                };
            }

            var wishListItemDtos = new List<WishListItemDto>();

            foreach (var wishListBook in wishList.Books)
            {
                var book = await _externalService.BookExists(wishListBook.BookId);
                if (book != null)
                {
                    wishListItemDtos.Add(new WishListItemDto
                    {
                        BookId = book.BookId,
                        BookName = book.Title,
                        Description = book.Description,
                        BookImage = book.Image,
                        Author = book.Author,
                        Price = book.Price
                    });
                }
            }

            return new Apiresponse<List<WishListItemDto>>
            {
                Success = true,
                Message = "Retrieved wishlist items successfully.",
                Data = wishListItemDtos
            };
        }

        public async Task<Apiresponse<string>> DeleteFromWishListAsync(int bookId)
        {
            int userId = _tokenService.GetUserIdFromToken();

            var wishList = await _wishListRepo.GetWishListByUserIdAsync(userId);
            if (wishList == null || !wishList.Books.Any())
            {
                return new Apiresponse<string>
                {
                    Success = false,
                    Message = "Wishlist is empty or does not exist."
                };
            }

            var bookToRemove = wishList.Books.FirstOrDefault(b => b.BookId == bookId);
            if (bookToRemove == null)
            {
                return new Apiresponse<string>
                {
                    Success = false,
                    Message = $"Book with ID {bookId} is not found in the wishlist."
                };
            }

            wishList.Books.Remove(bookToRemove);

            await _wishListRepo.SaveWishListAsync(wishList);

            return new Apiresponse<string>
            {
                Success = true,
                Message = $"Book with ID {bookId} was successfully removed from the wishlist."
            };
        }


    }
}
