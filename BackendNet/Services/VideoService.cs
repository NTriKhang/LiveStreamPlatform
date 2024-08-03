using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;

        public VideoService(IVideoRepository video)
        {
            _videoRepository = video;
        }

        public async Task<Videos> AddVideoAsync(Videos video, IFormFile thumbnail)
        {
            //var thumbnailPath = await _awsService.UploadImage(thumbnail);
            //if (thumbnailPath == null)
                //return null;
            //video.Thumbnail = thumbnailPath;
            //await _userService.UpdateStreamStatusAsync(video.User_id, StreamStatus.Streaming.ToString());
            return await _videoRepository.Add(video);
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
            return _videoRepository.GetByKey(nameof(Videos.Id), videoId);
        }

        public async Task<IEnumerable<Videos>> GetUserVideos(string userId, int page)
        {
            var additionalFilter = Builders<Videos>.Filter.Ne(nameof(Videos.Status), VideoStatus.Keep.ToString());             
            return await _videoRepository.GetManyByKey(nameof(Videos.User_id), userId, page,(int)PaginationCount.Video, additionalFilter);
        }

        public async Task UpdateVideoStatus(int status, string id)
        {
            var updateDefine = Builders<Videos>.Update.Set(x => x.StatusNum, status);
            await _videoRepository.UpdateByKey(nameof(Videos.Id), id, updateDefine);
        }

        public Task UpdateVideoView(string videoId)
        {
            try
            {
                var updateDefine = Builders<Videos>.Update.Inc(x => x.View, 1);
                _ = _videoRepository.UpdateByKey(nameof(Videos.Id), videoId, updateDefine);
                return Task.CompletedTask;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Videos>> GetNewestVideo(int page, int pageSize)
        {
            SortDefinition<Videos> sort = Builders<Videos>.Sort.Descending(x => x.Time);
            var filter = Builders<Videos>.Filter.Ne(u => u.StatusNum, (int)VideoStatus.TestData);

            return await _videoRepository.GetMany(page, pageSize , filter, sort);
        }

        public string GetIdYet()
        {
            return _videoRepository.GenerateKey();
        }
    }
}
