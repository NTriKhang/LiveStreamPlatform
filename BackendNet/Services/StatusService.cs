using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class StatusService : IStatusService
    {
        private readonly IStatusRepository statusRepository;
        public StatusService(IStatusRepository statusRepository)
        {
            this.statusRepository = statusRepository;
        }
        public async Task<Status> AddStatus(Status status)
        {
           return await statusRepository.Add(status);
        }

        public async Task<bool> DeleteStatus(string statusId)
        {
            return await statusRepository.RemoveByKey(nameof(Status.Id), statusId);
        }

        public async Task<IEnumerable<Status>> GetStatus(string ControllerCode)
        {
            return await statusRepository.GetManyByKey(nameof(Status.ControllerCode), ControllerCode);
        }

        public async Task<ReplaceOneResult> ReplaceStatus(Status status)
        {
            var filter = Builders<Status>.Filter.Eq(x => x.Id, status.Id);
            return await statusRepository.ReplaceAsync(filter, status);
        }
    }
}
