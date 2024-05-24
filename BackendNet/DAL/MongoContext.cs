using MongoDB.Driver;

namespace BackendNet.DAL
{
    public class MongoContext : IMongoContext
    {
        public MongoContext(ILiveStreamDatabaseSetting setting)
        {
            var mongoClient = new MongoClient(setting.ConnectionString);

            Database = mongoClient.GetDatabase(setting.DatabaseName);
        }

        public IMongoDatabase Database { get; set; }
    }
}
