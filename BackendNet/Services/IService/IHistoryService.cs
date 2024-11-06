using BackendNet.Dtos.Follow;
using BackendNet.Models;
using BackendNet.Setting;

namespace BackendNet.Services.IService
{
    public interface IHistoryService
    {
        Task<PaginationModel<History>> GetHistory(string userId, int page, int pageSize);
        Task<History> PostHistory(string userId, string videoId);
        Task<bool> RemoveHistory(string Id);
    }
}
