using System.Collections.Generic;
using System.Threading.Tasks;
using Kityme.Commands;
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
        public static IMongoCollection<PresetBorderGradient> BorderGradientPresetCollection { get; private set; }

        public static void Connect (string mongoURL)
        {
            Client = new MongoClient(mongoURL);
            Database = Client.GetDatabase("kityme");

            UserCollection = Database.UseCollection<User>();
            BorderGradientPresetCollection = Database.UseCollection<PresetBorderGradient>();
        }

        public static async Task ReplaceUserAsync(User user)
            => await UserCollection.ReplaceOneAsync(x => x.ID == user.ID, user);

        public static async Task<User> GetUserAsync(ulong id)
        {
            User user = await UserCollection.Find(x => x.ID == id).FirstOrDefaultAsync();
            if(user == null)
            {
                await CreateUserAsync(new User(id));
                return await GetUserAsync(id);
            } else
            {
                return user;
            }
        }

        public static async Task CreateUserAsync(User user)
            => await UserCollection.InsertOneAsync(user);

        public static async Task<List<PresetBorderGradient>> GetAllPresets()
            => await (await BorderGradientPresetCollection.FindAsync(_ => true)).ToListAsync<PresetBorderGradient>();

    }
}