using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IUserService
    {
        Task<Users> GetUserById(string id);
        Task<Users> AddUserAsync(Users user);
        Task<ReturnModel> AuthUser(string username, string password);
        Task<IEnumerable<Users>> GetUsersAsync();
        Task<UpdateResult> UpdateStreamStatusAsync(string user_id, string status);
        Task<bool> IsStreamKeyExist(string streamKey);
        Task<Users> GetUserByStreamKey(string streamKey);

        Task<SubUser> GetSubUser(string id);
        Task<bool> UpdateUser(Users user);
        Task<UpdateResult> UpdateStreamKey(string userId, Models.Submodel.StreamInfo streamInfo);
        Task<bool> IsStreamKeyInUse(string userId);
        Task<UpdateResult> UpdateUserAcitivity(string userId, CurrentActivity currentActivity)

    }
}
