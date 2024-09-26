using BackendNet.Models;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface ITrainModelService
    {
        Task UpdateInfo(string userId, string videoId);
        Task<PaginationModel<Videos>> OrderByInteraction(int page, int pageSize);

    }
}
