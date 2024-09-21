using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface ITrainModelService
    {
        public Task UpdateInfo(string userId, string videoId);
    }
}
