using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class RecommendRepository : Repository<Recommend>, IRecommendRepository
    {
        public RecommendRepository(IMongoContext context) : base(context)
        {
        }
    }
}
