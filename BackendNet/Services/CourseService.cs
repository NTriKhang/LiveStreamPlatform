using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository courseRepository;
        public CourseService(ICourseRepository courseRepository)
        {
            this.courseRepository = courseRepository;
        }

        public async Task<Course> AddCourse(Course course)
        {
            return await courseRepository.Add(course);
        }

        public Task<IEnumerable<Course>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<Course> GetCourse(string courseId)
        {
            return await courseRepository.GetByKey(nameof(Course._id), courseId);
        }

        public async Task<IEnumerable<Course>> GetCourses(string userId, int page, int pageSize)
        {
            SortDefinition<Course> sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            return await courseRepository.GetManyByKey("Created_user.user_id", userId, page, pageSize, true, sort);
        }

    }
}
