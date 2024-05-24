using BackendNet.Models;
using BackendNet.Repository.IRepositories;
using MongoDB.Driver;

namespace BackendNet.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<Users>
    {
        public Task<Users> AuthAsync(string username, string password);
        public Task<UpdateResult> UpdateStreamTokenAsync(string user_id, string status);
        public Task<bool> CompareKey(string user_id, string key);
        public Task<bool> IsTokenExist(string token);
    }
    
}
