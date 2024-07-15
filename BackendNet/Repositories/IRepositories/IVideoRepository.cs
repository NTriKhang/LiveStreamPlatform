using BackendNet.Models;
using BackendNet.Repository.IRepositories;

namespace BackendNet.Repositories.IRepositories
{
    public interface IVideoRepository : IRepository<Videos>
    {
        string GenerateKey();
        Task UpdateVideoStatus(int status, string id);
    }
}
