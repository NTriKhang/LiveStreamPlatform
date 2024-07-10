using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository followRepository;
        public FollowService(IFollowRepository followRepository)
        {
            this.followRepository = followRepository;
        }
        public async Task<IEnumerable<Follow>> GetFollower(string followed_id, int page)
        {
            return await followRepository.GetManyByKey(nameof(Follow.Followed) + '.' + nameof(Follow.Followed.user_id), followed_id, page, (int)PaginationCount.Follow, additionalFilter: null);
        }

        public async Task<IEnumerable<Follow>> GetFollowing(string follower_id, int page)
        {
            return await followRepository.GetManyByKey(nameof(Follow.Follower) + '.' + nameof(Follow.Follower.user_id), follower_id, page, (int)PaginationCount.Follow, additionalFilter: null);
        }

        public async Task<Follow> PostFollow(Follow follow)
        {
            return await followRepository.Add(follow);
        }

        public async Task<bool> RemoveFollow(string Id)
        {
            return await followRepository.RemoveByKey(nameof(Follow.Id), Id);
        }
    }
}
