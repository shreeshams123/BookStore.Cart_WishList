using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BookStore.Cart_WishList.Model.Entities
{
   
        public class WishList
        {
            [BsonId]
            public ObjectId Id { get; set; }

            public int UserId { get; set; }

            [BsonElement("Books")]
            public List<WishListBook> Books { get; set; } = new List<WishListBook>();
        }

        public class WishListBook
        {
            public int BookId { get; set; }
        }
    }
