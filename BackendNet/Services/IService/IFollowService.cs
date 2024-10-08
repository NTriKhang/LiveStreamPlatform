using BackendNet.Models;
using BackendNet.Setting;
using MongoDB.Bson;

namespace BackendNet.Services.IService
{
    public interface IFollowService
    {
        Task<PaginationModel<Follow>> GetFollower(string followed_id, int page);
        Task<PaginationModel<Follow>> GetFollowing(string follower_id, int page);
        Task<Follow> PostFollow(Follow follow);
        Task<bool> RemoveFollow(string Id);
        Task<BsonArray> GetFollowerEmail(string followedId);
        Task<int> GetTotalFollow(string userId);

    }
}
