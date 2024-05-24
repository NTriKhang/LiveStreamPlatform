using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class RoomRepository : Repository<Rooms>, IRoomRepository
    {
        public RoomRepository(IMongoContext context) : base(context)
        {
            
        }
    }
}
