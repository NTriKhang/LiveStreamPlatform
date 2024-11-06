using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class HistoryRepository : Repository<History>, IHIstoryRepository
    {
        public HistoryRepository(IMongoContext context) : base(context)
        {
        }
    }
}
