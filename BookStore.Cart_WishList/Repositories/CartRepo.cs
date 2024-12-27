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
            return await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task SaveCartAsync(Cart cart)
        {
            if (cart.Id == ObjectId.Empty)
            {
                cart.Id = ObjectId.GenerateNewId(); 
            }
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, cart.UserId);
            await _cartCollection.ReplaceOneAsync(filter, cart, new ReplaceOptions { IsUpsert = true });
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

