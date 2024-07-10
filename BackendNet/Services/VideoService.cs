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
        private readonly IUserService _userService;
        private readonly IAwsService _imageService;

        public VideoService(IVideoRepository video, IAwsService imageService, IUserService userService)
        {
            _videoRepository = video;
            _imageService = imageService;
            _userService = userService;
        }

        public async Task<Videos> AddVideoAsync(Videos video, IFormFile thumbnail)
        {
            var thumbnailPath = await _imageService.UploadImage(thumbnail);
            if (thumbnailPath == null)
                return null;
            video.Thumbnail = thumbnailPath;
            await _userService.UpdateStreamStatusAsync(video.User_id, StreamStatus.Streaming.ToString());
            return await _videoRepository.Add(video);
        }

        public async Task<IEnumerable<Videos>> GetFollowingVideos(string userId, int page)
        {
            var additionalFilter = Builders<Videos>.Filter.Ne(nameof(Videos.Status), VideoStatus.Keep.ToString());             
            return await _videoRepository.GetManyByKey(nameof(Videos.User_id), userId, page,(int)PaginationCount.Video, additionalFilter);throw new NotImplementedException();
        }

        public Task<Videos> GetVideoAsync(string videoId)
        {
            return _videoRepository.GetByKey(nameof(Videos.Id), videoId);
        }

        public async Task<IEnumerable<Videos>> GetVideos(string userId, int page)
        {
            var additionalFilter = Builders<Videos>.Filter.Ne(nameof(Videos.Status), VideoStatus.Keep.ToString());             
            return await _videoRepository.GetManyByKey(nameof(Videos.User_id), userId, page,(int)PaginationCount.Video, additionalFilter);
        }

        public async Task UpdateVideoStatus(string status, string id)
        {
            await _videoRepository.UpdateVideoStatus(status, id);
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
    }
}
