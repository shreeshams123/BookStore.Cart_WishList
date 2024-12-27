namespace BookStore.Cart_WishList.Model.DTOs
{

    public class BookDataDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
    }
    public class BookResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BookDataDto Data { get; set; }
    }
}
