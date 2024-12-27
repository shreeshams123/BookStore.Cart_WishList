using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BookStore.Cart_WishList.Model.Entities
{
    public class Cart
    {
        [BsonId]
        public ObjectId Id { get; set; } 

        public int UserId { get; set; } 

        [BsonElement("Books")]
        public List<CartBook> Books { get; set; } = new List<CartBook>(); 

        public decimal TotalPrice { get; set; } 
        public int TotalQuantity { get; set; } 
    }

    public class CartBook
    {
        public int BookId { get; set; } 
        public int Quantity { get; set; } 
    }
}
