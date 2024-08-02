using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface IVideoService
    {
        Task<Videos> AddVideoAsync(Videos video, IFormFile thumbnail);
        Task<Videos> AddVideoAsync(Videos video);
        Task<Videos> GetVideoAsync(string videoId);
        Task<IEnumerable<Videos>> GetNewestVideo(int page, int pageSize);
        Task<IEnumerable<Videos>> GetUserVideos(string userId, int page);
        Task UpdateVideoStatus(int status, string id);
        Task UpdateVideoView(string videoId);
        Task<bool> RemoveVideo(string Id);
        string GetIdYet();

    }
}
