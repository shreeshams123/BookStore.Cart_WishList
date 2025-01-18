using BookStore.Cart_WishList.Model.Entities;
using MongoDB.Driver;

namespace BookStore.Cart_WishList.Data
{
    public class MongoDbService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;
        public MongoDbService(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration["ConnectionStrings:DbConnection"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
            }

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);

            var databaseName = mongoUrl.DatabaseName ?? "DefaultDatabase";
            _database = mongoClient.GetDatabase(databaseName);
        }

        public IMongoDatabase? Database => _database;
    }
}
