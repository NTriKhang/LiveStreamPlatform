using BackendNet.Dtos.Follow;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace BackendNet.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository followRepository;
        private readonly IUserService userService;
        public FollowService(
            IFollowRepository followRepository
            , IUserService userService
        )
        {
            this.followRepository = followRepository;
            this.userService = userService;
        }
        public async Task<int> GetTotalFollow(string userId)
        {
            var filter = Builders<Follow>.Filter.Eq(x => x.Followed.user_id, userId);
            var res = await followRepository.GetAll(filter, null);
            return res.Count();
        }
        public async Task<PaginationModel<Follow>> GetFollower(string followed_id, int page)
        {

            return await followRepository.GetManyByKey(nameof(Follow.Followed) + '.' + nameof(Follow.Followed.user_id), followed_id, page, (int)PaginationCount.Follow, additionalFilter: null);
        }

        public async Task<BsonArray> GetFollowerEmail(string followedId)
        {
            var matchStage = new BsonDocument("$match", new BsonDocument()
            {
                { "Followed.user_id", new ObjectId(followedId) }

            });
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

            var res = await followRepository.ExecAggre(new[] { matchStage, lookupStage, unwindStage, groupStage });
            if(res.Any())
            {
                var doc = res.First();
                var emails = doc["emails"].AsBsonArray;
                return emails;
            }
            return null;
        }

        public async Task<PaginationModel<Follow>> GetFollowing(string follower_id, int page)
        {
            return await followRepository.GetManyByKey(nameof(Follow.Follower) + '.' + nameof(Follow.Follower.user_id), follower_id, page, (int)PaginationCount.Follow, additionalFilter: null);
        }

        public async Task<Follow> PostFollow(FollowPostDto followDto, string userId)
        {
            var followerTask = userService.GetSubUser(userId);
            var followedTask = userService.GetSubUser(followDto.userId);

            try
            {
                await Task.WhenAll(followedTask, followerTask);
            }
            catch (Exception)
            {
                return null;
            }

            if (!followedTask.IsCompletedSuccessfully || !followerTask.IsCompletedSuccessfully)
                return null;

            var followed = await followedTask;
            var follower = await followerTask;

            var follow = new Follow()
            {
                FollowDate = DateTime.UtcNow,
                Followed = new FollowInfo(followed.user_id, followed.user_name, followed.user_avatar),
                Follower = new FollowInfo(follower.user_id, follower.user_name, follower.user_avatar)
            };

            return await followRepository.Add(follow);
        }

        public async Task<bool> RemoveFollow(string Id)
        {
            return await followRepository.RemoveByKey(nameof(Follow.Id), Id);
        }
    }
}
