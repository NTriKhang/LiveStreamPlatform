using BackendNet.Models;
using MongoDB.Bson;

namespace BackendNet.Services.IService
{
    public interface IFollowService
    {
        Task<IEnumerable<Follow>> GetFollower(string followed_id, int page);
        Task<IEnumerable<Follow>> GetFollowing(string follower_id, int page);
        Task<Follow> PostFollow(Follow follow);
        Task<bool> RemoveFollow(string Id);
        Task<BsonArray> GetFollowerEmail(string followedId);
    }
}
