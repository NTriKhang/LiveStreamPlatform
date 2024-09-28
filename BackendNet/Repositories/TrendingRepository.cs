using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class TrendingRepository : Repository<Trending>, ITrendingRepository
    {
        public TrendingRepository(IMongoContext context) : base(context)
        {
        }
    }
}
