using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;
using BookStore.Cart_WishList.Repositories;
using BookStore.Cart_WishList.Services;

namespace BookStore.Cart_WishList.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepo _cartRepo;
        private readonly TokenService _tokenService;
        private readonly IExternalService _externalService;
        public CartService(ICartRepo cartRepo, TokenService tokenService, IExternalService externalService)
        {
            _cartRepo = cartRepo;
            _tokenService = tokenService;
            _externalService = externalService;
        }

        public async Task<Apiresponse<Cart>> AddToCartAsync(AddToCartRequestDto requestDto)
        {
            var book = await _externalService.BookExists(requestDto.BookId);
            int userId = _tokenService.GetUserIdFromToken();

            if (book == null)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = $"Book with ID {requestDto.BookId} does not exist."
                };
            }

            if (book.StockQuantity < requestDto.Quantity)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = $"Requested quantity ({requestDto.Quantity}) exceeds available stock ({book.StockQuantity})."
                };
            }

            var cart = await _cartRepo.GetCartByUserIdAsync(userId)
                      ?? new Cart { UserId = userId };

            var existingBook = cart.Books.FirstOrDefault(b => b.BookId == requestDto.BookId);
            if (existingBook != null)
            {
                if (existingBook.Quantity + requestDto.Quantity > book.StockQuantity)
                {
                    return new Apiresponse<Cart>
                    {
                        Success = false,
                        Message = $"Total quantity ({existingBook.Quantity + requestDto.Quantity}) exceeds available stock ({book.StockQuantity})."
                    };
                }

                existingBook.Quantity += requestDto.Quantity;
            }
            else
            {
                cart.Books.Add(new CartBook
                {
                    BookId = requestDto.BookId,
                    Quantity = requestDto.Quantity
                });
            }

            cart.TotalQuantity = cart.Books.Sum(b => b.Quantity);
            cart.TotalPrice = cart.Books.Sum(b => b.Quantity * book.Price);

            await _cartRepo.SaveCartAsync(cart);

            return new Apiresponse<Cart>
            {
                Success = true,
                Message = "Book added to cart successfully.",
                Data = cart
            };
        }

        public async Task<Apiresponse<Cart>> UpdateCartAsync(AddToCartRequestDto requestDto)
        {
            var book = await _externalService.BookExists(requestDto.BookId);
            int userId = _tokenService.GetUserIdFromToken();

            if (book == null)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = $"Book with ID {requestDto.BookId} does not exist."
                };
            }

            if (requestDto.Quantity > book.StockQuantity || requestDto.Quantity < 0)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = $"Invalid quantity"
                };
            }

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = "Cart not found."
                };
            }

            var existingBook = cart.Books.FirstOrDefault(b => b.BookId == requestDto.BookId);
            if (existingBook == null)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = "Book not found in the cart."
                };
            }

            if (requestDto.Quantity == 0)
            {
                cart.Books.Remove(existingBook);
            }
            else
            {
                existingBook.Quantity = requestDto.Quantity;
            }

            cart.TotalQuantity = cart.Books.Sum(b => b.Quantity);
            cart.TotalPrice = cart.Books.Sum(b => b.Quantity * book.Price);

            await _cartRepo.SaveCartAsync(cart);

            return new Apiresponse<Cart>
            {
                Success = true,
                Message = "Cart updated successfully.",
                Data = cart
            };
        }
        public async Task<Apiresponse<CartDetailsDto>> GetAllItemsFromCartAsync()
        {
            int userId = _tokenService.GetUserIdFromToken();

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Books.Any())
            {
                return new Apiresponse<CartDetailsDto>
                {
                    Success = false,
                    Message = "No items found in the cart."
                };
            }

            var cartItemDtos = new List<CartItemDto>();

            foreach (var cartBook in cart.Books)
            {
                var book = await _externalService.BookExists(cartBook.BookId);
                if (book != null)
                {
                    cartItemDtos.Add(new CartItemDto
                    {
                        BookId = book.BookId,
                        BookName = book.Title,
                        Description = book.Description,
                        BookImage = book.Image,
                        Author = book.Author,
                        Price = book.Price,
                        Quantity = cartBook.Quantity,
                        TotalPrice = cartBook.Quantity * book.Price
                    });
                }
            }

            var totalQuantity = cartItemDtos.Sum(item => item.Quantity);
            var totalPrice = cartItemDtos.Sum(item => item.TotalPrice);

            return new Apiresponse<CartDetailsDto>
            {
                Success = true,
                Message = "Retrieved cart details successfully.",
                Data = new CartDetailsDto
                {
                    CartItems = cartItemDtos,
                    TotalQuantity = totalQuantity,
                    TotalPrice = totalPrice
                }
            };
        }

        public async Task<Apiresponse<Cart>> DeleteFromCartAsync(int bookId)
        {
            int userId = _tokenService.GetUserIdFromToken();

            var cart = await _cartRepo.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Books.Any())
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = "No items found in the cart."
                };
            }

            var cartBook = cart.Books.FirstOrDefault(b => b.BookId == bookId);
            if (cartBook == null)
            {
                return new Apiresponse<Cart>
                {
                    Success = false,
                    Message = "Book not found in the cart."
                };
            }

            cart.Books.Remove(cartBook);

            decimal totalPrice = 0;
            foreach (var book in cart.Books)
            {
                var bookData = await _externalService.BookExists(book.BookId);
                if (bookData != null)
                {
                    totalPrice += book.Quantity * bookData.Price;
                }
            }

            cart.TotalQuantity = cart.Books.Sum(b => b.Quantity);
            cart.TotalPrice = totalPrice;
            if (!cart.Books.Any())
            {
                await _cartRepo.DeleteCartAsync(cart.Id.ToString()); 
                return new Apiresponse<Cart>
                {
                    Success = true,
                    Message = "Cart is empty and has been deleted."
                };
            }


            await _cartRepo.SaveCartAsync(cart);

            return new Apiresponse<Cart>
            {
                Success = true,
                Message = "Book successfully removed from the cart.",
                Data = cart
            };
        }

    }
}


