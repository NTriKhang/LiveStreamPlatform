using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IUserService
    {
        Task<SubUser> GetSubUser(string id);
        Task<Users> GetUserById(string id);
        Task<Users> GetUserByStreamKey(string streamKey);
        Task<PaginationModel<Users>> GetUsersAsync(int page, int pageSize, string? userName = null);

        Task<Users> AddUserAsync(Users user);

        Task<bool> UpdateUser(Users user);
        Task<UpdateResult> UpdateIncome(string userId, SubTrade subIncome);
        Task<UpdateResult> UpdateOutcome(string userId, SubTrade subOutcome);
        Task<UpdateResult> UpdateStreamKey(string userId, Models.Submodel.StreamInfo streamInfo);
        Task<UpdateResult> UpdateUserAcitivity(string userId, CurrentActivity currentActivity);
        Task<UpdateResult> UpdateStreamStatusAsync(string user_id, string status);

        Task<ReturnModel> AuthUser(string username, string password);

        Task<bool> IsStreamKeyExist(string streamKey);
        Task<bool> IsStreamKeyValid(string userId);
    }
}
