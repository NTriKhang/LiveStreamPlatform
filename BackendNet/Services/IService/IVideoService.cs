using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface IVideoService
    {
        Task<Videos> AddVideoAsync(Videos video, IFormFile thumbnail);
        Task<Videos> GetVideoAsync(string videoId);
        Task<IEnumerable<Videos>> GetVideos(string userId, int page);
        Task<IEnumerable<Videos>> GetFollowingVideos(string userId, int page);
        Task UpdateVideoStatus(string status, string id);
        Task UpdateVideoView(string videoId);

    }
}
