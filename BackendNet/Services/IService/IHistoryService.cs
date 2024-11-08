using BackendNet.Dtos.Follow;
using BackendNet.Models;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IHistoryService
    {
        Task<PaginationModel<History>> GetHistoryByFilter(int page, int pageSize, FilterDefinition<History> filterDefinition, SortDefinition<History> sortDefinition);
        Task<PaginationModel<History>> GetHistory(string userId, int page, int pageSize);
        Task<History> PostHistory(string userId, string videoId);
        Task<bool> RemoveHistory(string Id);
    }
}
