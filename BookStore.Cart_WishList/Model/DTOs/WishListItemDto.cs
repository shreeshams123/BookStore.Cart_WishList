namespace BookStore.Cart_WishList.Model.DTOs
{
    public class WishListItemDto
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Description { get; set; }
        public string BookImage { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
    }
}
