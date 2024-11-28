using BackendNet.Dtos.Redis;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Mvc;
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
        public VideoService(
            IVideoRepository video
            , IConnectionMultiplexer redisConnect
        )
        {
            _videoRepository = video;
            _redisConnect = redisConnect;
        }

        public async Task<PaginationModel<Videos>> SearchVideo(
            int page
            , int pageSize
            , string title)
        {
            var filDef = Builders<Videos>.Filter.Eq(x => x.Title, title);
            return await _videoRepository.GetManyByFilter(page, pageSize, filDef, Builders<Videos>.Sort.Descending(x => x.Time));
        }
        public Task<Videos> GetVideoAsync(string videoId)
        {
            var filDef = Builders<Videos>.Filter.Eq(x => x.Id, videoId);
            return _videoRepository.GetByFilter(filDef);
        }
        public async Task<SubVideo> GetSubVideo(string videoId)
        {
            var filter = Builders<Videos>.Filter.Eq(x => x.Id, videoId);

            var video = await _videoRepository.GetByFilter(filter);
            return new SubVideo(video.Id, video.Title, video.Thumbnail, video.Tags, video.Time ?? DateTime.Now);
        }
        public async Task<PaginationModel<Videos>> GetUserVideos(int page, int pageSize, string userId)
        {
            //var additionalFilter = Builders<Videos>.Filter.Ne(nameof(Videos.Status), VideoStatus.Keep.ToString());
            return await _videoRepository.GetManyByFilter(page, pageSize,
                Builders<Videos>.Filter.Eq(x => x.User_id, userId),
                Builders<Videos>.Sort.Descending(x => x.Time)
            );
        }
        public async Task<PaginationModel<Videos>> GetNewestVideo(int page, int pageSize)
        {
            SortDefinition<Videos> sort = Builders<Videos>.Sort.Descending(x => x.Time);

            return await _videoRepository.GetMany(page, pageSize, null, sort);
        }
        public async Task<PaginationModel<Videos>> GetRecommendVideo(
            int page
            , int pageSize
            , List<string> recentVideoIds)
        {
            try
            {
                var videoFilter = Builders<Videos>.Filter.In(x => x.Id, recentVideoIds);

                var recentVideos = await _videoRepository.GetAll(videoFilter, null);

                var preferredTags = recentVideos
                   .SelectMany(video => video.Tags.Select(tag => tag.ToString()))
                   .GroupBy(tag => tag)
                   .OrderByDescending(group => group.Count())
                   .Take(3)
                   .Select(group => group.Key)
                   .ToList();

                var unseenVideoFilter = Builders<Videos>.Filter.And(
                    Builders<Videos>.Filter.Nin(x => x.Id, recentVideoIds),
                    Builders<Videos>.Filter.AnyIn(x => x.Tags, preferredTags)
                );
                var recommendations = await _videoRepository.GetManyByFilter(page, pageSize, unseenVideoFilter, null);

                return recommendations;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public string GetAvailableId()
        {
            return _videoRepository.GenerateKey();
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
        public async Task<bool> RemoveVideo(string Id)
        {
            return await _videoRepository.RemoveByKey(nameof(Follow.Id), Id);
        }

        public async Task UpdateLike(string videoId, bool isLike)
        {
            var filter = Builders<Videos>.Filter.Eq(x => x.Id, videoId);
            UpdateDefinition<Videos> updateDefine = null;
            if (isLike)
            {
                updateDefine = Builders<Videos>.Update.Inc(x => x.Like, 1);

            }
            else {
                updateDefine = Builders<Videos>.Update.Inc(x => x.Like, -1);

            }
            await _videoRepository.UpdateByFilter(filter, updateDefine);
        }
    }
}
