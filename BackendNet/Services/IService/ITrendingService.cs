using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface ITrendingService
    {
        public Task ProcessTrend(string videoId, int videoView);
    }
}
