using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;

namespace BackendNet.DAL
{
    public class MongoContext : IMongoContext
    {
        public MongoContext(ILiveStreamDatabaseSetting setting)
        {

            var settings = MongoClientSettings.FromConnectionString(setting.ConnectionString);
            //settings.ClusterConfigurator = cb =>
            //{
            //    cb.Subscribe<CommandStartedEvent>(e =>
            //    {
            //        Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
            //    });
            //};

            var mongoClient = new MongoClient(settings);

            Database = mongoClient.GetDatabase(setting.DatabaseName);
        }

        public IMongoDatabase Database { get; set; }
    }
}
