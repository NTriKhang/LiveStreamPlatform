using BackendNet.Models;
using BackendNet.Setting;

namespace BackendNet.Services.IService
{
    public interface ITrendingService
    {
        public Task ProcessTrend(string videoId, int videoView);
        public Task<PaginationModel<Videos>> GetTrendingVideo(int page, int pageSize);

    }
}
