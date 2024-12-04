using BackendNet.Models;
using BackendNet.Repository.IRepositories;

namespace BackendNet.Repositories.IRepositories
{
    public interface ISysRelationshipRepo : IRepository<SysRelationship>
    {
        Task UpdateUserMonitor(Users user);
    }
}
