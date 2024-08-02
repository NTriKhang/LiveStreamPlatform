using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class VideoRepository : Repository<Videos>, IVideoRepository
    {
        public VideoRepository(IMongoContext context) : base(context)
        {
            
        }

        public string GenerateKey()
        {
            ObjectIdGenerator objectIdGenerator = new ObjectIdGenerator();
            var id = ObjectId.GenerateNewId().ToString();
            return id;
        }

        public async Task UpdateVideoStatus(int status, string videoId)
        {
            var update = Builders<Videos>.Update.Set(nameof(Videos.StatusNum), status);
            await _collection.UpdateOneAsync(x => x.Id == videoId, update);
        }
    }
}
