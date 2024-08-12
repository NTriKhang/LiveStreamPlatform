using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(IMongoContext context) : base(context)
        {
        }

        public Task<ActionResult> AddVideoToCrs(Videos videos)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult> DeleteVideoFromCrs(string videoId)
        {
            throw new NotImplementedException();
        }
    }
}
