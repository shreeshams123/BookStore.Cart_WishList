using BookStore.Cart_WishList.Data;
using BookStore.Cart_WishList.Model.DTOs;
using BookStore.Cart_WishList.Model.Entities;
using BookStore.Cart_WishList.Model;
using MongoDB.Driver;
using BookStore.Cart_WishList.Interfaces;


namespace BookStore.Cart_WishList.Repositories
{
    public class WishListRepo:IWishListRepo
    {
        private readonly IMongoCollection<WishList> _wishListCollection;

        public WishListRepo(MongoDbService mongoDbService)
        {
            _wishListCollection = mongoDbService.Database.GetCollection<WishList>("wishlist");
        }
        public async Task<WishList> GetWishListByUserIdAsync(int userId)
        {
            return await _wishListCollection
                .Find(w => w.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task SaveWishListAsync(WishList wishList)
        {
            var filter = Builders<WishList>.Filter.Eq(c => c.UserId, wishList.UserId);
            await _wishListCollection.ReplaceOneAsync(filter, wishList, new ReplaceOptions { IsUpsert = true });
        }
        
    }
}
