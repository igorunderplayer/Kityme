using System.Threading.Tasks;
using Kityme.Entities;
using Kityme.Extensions;
using MongoDB.Driver;


namespace Kityme.Managers
{
    public static class DBManager
    {
        public static IMongoClient Client { get; private set; }
        public static IMongoDatabase Database { get; private set; }
        public static IMongoCollection<User> UserCollection { get; private set; }

        public static void Connect (string mongoURL)
        {
            Client = new MongoClient(mongoURL);
            Database = Client.GetDatabase("kityme");

            UserCollection = Database.UseCollection<User>();
        }

        public static async Task ReplaceUserAsync(User user)
            => await UserCollection.ReplaceOneAsync(x => x.ID == user.ID, user);

        public static async Task<User> GetUserAsync(ulong id)
            => await UserCollection.Find(x => x.ID == id).FirstOrDefaultAsync();

        public static async Task CreateUserAsync(User user)
            => await UserCollection.InsertOneAsync(user);

    }
}