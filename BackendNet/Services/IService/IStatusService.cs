using BackendNet.Models;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IStatusService
    {
        Task<IEnumerable<Status>> GetStatus(string ControllerCode);
        Task<Status> AddStatus(Status status);
        Task<ReplaceOneResult> ReplaceStatus(Status status);
        Task<bool> DeleteStatus(string statusId);
    }
}
