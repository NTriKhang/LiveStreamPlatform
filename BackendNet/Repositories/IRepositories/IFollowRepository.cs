using BackendNet.Models;
using BackendNet.Repository.IRepositories;

namespace BackendNet.Repositories.IRepositories
{
    public interface IFollowRepository : IRepository<Follow>
    {
        Task<IEnumerable<Follow>> GetFollowerEmail(string followedId);

    }
}
