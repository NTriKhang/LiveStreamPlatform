using BackendNet.Models;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IUserService
    {
        Task<Users> GetUserById(string id);
        Task<Users> AddUserAsync(Users user);
        Task<Users> AuthUser(string username, string password);
        Task<IEnumerable<Users>> GetUsersAsync();
        Task<UpdateResult> UpdateStreamStatusAsync(string user_id, string status);
        Task<bool> IsTokenExist(string streamKey);
        Task<Users> GetUserByStreamKey(string streamKey);


    }
}
