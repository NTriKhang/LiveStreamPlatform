using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class CrsContentRepository : Repository<CourseContent>, ICrsContentRepository
    {
        public CrsContentRepository(IMongoContext context) : base(context)
        {
        }
    }
}
