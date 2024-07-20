using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class FollowRepository : Repository<Follow>, IFollowRepository
    {
        public FollowRepository(IMongoContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Follow>> GetFollowerEmail(string followedId)
        {
            var queryableCollection = _collection.AsQueryable();
            var query = queryableCollection
                .Where(x => x.Followed.user_id == followedId)
                .ToList();
            return query;
        }

    }
}
