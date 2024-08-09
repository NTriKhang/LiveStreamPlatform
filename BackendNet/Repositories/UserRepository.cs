using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class UserRepository : Repository<Users>, IUserRepository
    {
        public UserRepository(IMongoContext context) : base(context)
        {

        }
        public async Task<UpdateResult> UpdateStreamTokenAsync(string user_id, string status)
        {
            var update = Builders<Users>.Update.Set("StreamInfo.Status", status);
            return await _collection.UpdateOneAsync(x => x.Id == user_id, update);
        }

        public async Task<Users> AuthAsync(string username, string password)
        {
            var user = await _collection.Find(user => user.UserName == username && user.Password == password).ToListAsync();
            return user.FirstOrDefault()!;
        }

        public async Task<bool> CompareKey(string user_id, string key)
        {
            var res = await _collection.FindAsync(user => user.Id == user_id && user.StreamInfo.Stream_token == key);
            return res.Current != null;
        }

        public async Task<bool> IsTokenExist(string token)
        {
            if((await GetByKey(nameof(Users.StreamInfo.Stream_token), token)) != null)
            {
                return true;
            }
            return false;
        }
    }
}
