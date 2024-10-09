using BackendNet.Dtos.Redis;
using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace BackendNet.Services
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IConnectionMultiplexer _redisConnect;
        private readonly ITrendingService _trendingService;
        public VideoService(
            IVideoRepository video
            , ITrendingService trendingService
            , IConnectionMultiplexer redisConnect

        )
        {
            _videoRepository = video;
            _redisConnect = redisConnect;
            _trendingService = trendingService;
        }
        public async Task<Videos> AddVideoAsync(Videos video)
        {
            try
            {
                return await _videoRepository.Add(video);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> RemoveVideo(string Id)
        {
            return await _videoRepository.RemoveByKey(nameof(Follow.Id), Id);
        }
        public Task<Videos> GetVideoAsync(string videoId)
        {
            var filDef = Builders<Videos>.Filter.Eq(x => x.Id, videoId);
            return _videoRepository.GetByFilter(filDef);
        }
        public async Task<PaginationModel<Videos>> GetUserVideos(int page, int pageSize, string userId)
        {
            //var additionalFilter = Builders<Videos>.Filter.Ne(nameof(Videos.Status), VideoStatus.Keep.ToString());
            return await _videoRepository.GetManyByFilter(page, pageSize,
                Builders<Videos>.Filter.Eq(x => x.User_id, userId),
                Builders<Videos>.Sort.Descending(x => x.Time)
            );
        }
        public async Task UpdateVideoStatus(int status, string id)
        {
            var filter = Builders<Videos>.Filter.Eq(x => x.Id, id);
            var updateDefine = Builders<Videos>.Update.Set(x => x.StatusNum, status);
            await _videoRepository.UpdateByFilter(filter, updateDefine);
        }
        public Task UpdateVideoView(string videoId)
        {
            try
            {
                var filter = Builders<Videos>.Filter.Eq(x => x.Id, videoId);
                var updateDefine = Builders<Videos>.Update.Inc(x => x.View, 1);
                _ = _videoRepository.UpdateByFilter(filter, updateDefine);
                return Task.CompletedTask;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<PaginationModel<Videos>> GetNewestVideo(int page, int pageSize)
        {
            SortDefinition<Videos> sort = Builders<Videos>.Sort.Descending(x => x.Time);

            return await _videoRepository.GetMany(page, pageSize, null, sort);
        }
        public async Task<PaginationModel<Videos>> GetRecommendVideo(int page, int pageSize, string userId)
        {
            var db = _redisConnect.GetDatabase();
            try
            {
                var redisVal = await db.StringGetAsync("svd_predictions");

                var recModel = JsonConvert.DeserializeObject<List<RecommendModel>>(redisVal)
                    .Where(x => x.user_id == userId)
                    .OrderByDescending(x => x.predicted_rating)
                    .Select(x => x.item_id)
                    .Skip((pageSize - 1) * page)
                    .Take(pageSize)
                    .ToList();

                var filterDef = Builders<Videos>.Filter.In(x => x.Id, recModel);
                return await _videoRepository.GetManyByFilter(page, pageSize, filterDef, null);
            }
            catch (Exception)
            {
                //await db.KeyDeleteAsync(user.Id);
                throw;
            }
        }
        public string GetAvailableId()
        {
            return _videoRepository.GenerateKey();
        }


    }
}
