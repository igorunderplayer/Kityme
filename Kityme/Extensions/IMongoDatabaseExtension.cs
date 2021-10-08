using MongoDB.Bson;
using MongoDB.Driver;

namespace  Kityme.Extensions
{
    public static class IMongoDatabaseExtension
        {
            public static IMongoCollection<T> UseCollection<T>(this IMongoDatabase database)
            {
                var filter = new ListCollectionNamesOptions { Filter = Builders<BsonDocument>.Filter.Eq("name", typeof(T).Name) };
                if (!database.ListCollectionNames(filter).Any()) database.CreateCollection(typeof(T).Name);
                return database.GetCollection<T>(typeof(T).Name);
            }
        }
}