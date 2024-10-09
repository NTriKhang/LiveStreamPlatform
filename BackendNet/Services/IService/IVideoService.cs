using BackendNet.Models;
using BackendNet.Setting;

namespace BackendNet.Services.IService
{
    public interface IVideoService
    {
        Task<Videos> AddVideoAsync(Videos video);
        Task<Videos> GetVideoAsync(string videoId);
        Task<PaginationModel<Videos>> GetNewestVideo(int page, int pageSize);
        Task<PaginationModel<Videos>> GetRecommendVideo(int page, int pageSize, string userId);
        Task<PaginationModel<Videos>> GetUserVideos(int page, int pageSize, string userId);
        Task UpdateVideoStatus(int status, string id);
        Task UpdateVideoView(string videoId);
        Task<bool> RemoveVideo(string Id);
        string GetAvailableId();

    }
}
