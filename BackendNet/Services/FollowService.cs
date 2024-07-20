using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Bson;
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

        public async Task<IEnumerable<Follow>> GetFollowerEmail(string followedId)
        {
            var matchStage = new BsonDocument
            {
                {
                    "$match",
                    new BsonDocument
                    {
                        { "Followed.user_id", new ObjectId(followedId) }
                    }
                }
            };
            var lookupStage = new BsonDocument
            {
                {
                    "$lookup",
                    new BsonDocument
                    {
                        { "from", "Users" },
                        { "localField", "Follower.user_id" },
                        { "foreignField", "_id" },
                        { "as", "FollowerDetail" }
                    }
                }
            };
            var unwindStage = new BsonDocument
            {
                {
                    "$unwind", "$FollowerDetail"
                }
            };
            var groupStage = new BsonDocument
            {
                {
                    "$group", new BsonDocument
                    {
                        { "_id", "$Followed.user_id" },
                        { "emails", new BsonDocument
                            {
                                { "$push", "$FollowerDetail.Email" }
                            }
                        }
                    }
                }
            };
            //var matchFilter = Builders<Follow>.Filter.Eq(r => r.Followed.user_id, followedId);
            //// Defines the aggregation pipeline with the $match and $group aggregation stages
            //var pipeline = new EmptyPipelineDefinition<Follow>()
            //    .Match(matchFilter);
            return await followRepository.GetFollowerEmail(followedId);
            //var res = await followRepository.ExecAggre(new[] { matchStage });
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
