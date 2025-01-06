namespace BookStore.Cart_WishList.Model.DTOs
{
        public class CartItemDto
        {
            public int BookId { get; set; }
            public string title { get; set; }
            public string Description {  get; set; }
            public string Image { get; set; } 
            public string Author { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice {  get; set; }
        }
    public class CartDetailsDto
    {
        public List<CartItemDto> CartItems { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

}

