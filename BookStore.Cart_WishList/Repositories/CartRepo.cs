using BookStore.Cart_WishList.Data;
using BookStore.Cart_WishList.Interfaces;
using BookStore.Cart_WishList.Model;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BookStore.Cart_WishList.Repositories
{
    public class CartRepo : ICartRepo
    {
        private readonly IMongoCollection<Cart> _cartCollection;

        public CartRepo(MongoDbService mongoDbService)
        {
            _cartCollection = mongoDbService.Database.GetCollection<Cart>("cart");
        }
        public async Task<Cart> GetCartByUserIdAsync(int userId)
        {
            var filter = Builders<BookStore.Cart_WishList.Model.Entities.Cart>.Filter.Eq(c => c.UserId, userId);
            var findFluent = _cartCollection.Find(filter);

            // Manually implement behavior of FirstOrDefaultAsync here
            var cart = await findFluent.FirstOrDefaultAsync();
            return cart;
        }

        public async Task SaveCartAsync(Cart cart)
        {
            if (cart.Id == ObjectId.Empty)
            {
                cart.Id = ObjectId.GenerateNewId();
            }

            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, cart.UserId);

            // Prepare update definition for all fields
            var update = Builders<Cart>.Update
                .Set(c => c.UserId, cart.UserId)
                .Set(c => c.Books, cart.Books)
                .Set(c => c.TotalPrice, cart.TotalPrice)
                .Set(c => c.TotalQuantity, cart.TotalQuantity);

            // Perform upsert operation
            var result = await _cartCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

            // Enhanced condition to validate operation success
            if (result.MatchedCount == 0 && result.UpsertedId == BsonValue.Create(null))
            {
                throw new Exception($"No cart updated or inserted for UserId {cart.UserId}.");
            }

            // Logging for debugging purposes
            if (result.UpsertedId != BsonValue.Create(null))
            {
                Console.WriteLine($"New cart created for UserId {cart.UserId} with ID {result.UpsertedId}.");
            }
            else if (result.ModifiedCount > 0)
            {
                Console.WriteLine($"Cart updated for UserId {cart.UserId}.");
            }
            else
            {
                Console.WriteLine($"No changes were made to the cart for UserId {cart.UserId}.");
            }
        }




        public async Task DeleteCartAsync(string cartId)
        {
            var objectId = ObjectId.Parse(cartId); 

            var filter = Builders<Cart>.Filter.Eq(c => c.Id, objectId);

            var result = await _cartCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new Exception($"No cart found with ID {cartId}.");
            }
        }
    }

    }

