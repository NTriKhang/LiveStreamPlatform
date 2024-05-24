using MongoDB.Driver;

namespace BackendNet.DAL
{
    public interface IMongoContext
    {
        IMongoDatabase Database { get; set; }
    }
}
