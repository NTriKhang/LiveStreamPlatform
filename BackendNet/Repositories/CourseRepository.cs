using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(IMongoContext context) : base(context)
        {
        }
    }
}
