using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class FollowRepository : Repository<Follow>, IFollowRepository
    {
        public FollowRepository(IMongoContext context) : base(context)
        {
        }
    }
}
