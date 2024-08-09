using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAll();
        Task<IEnumerable<Course>> GetCourses(string userId, int page, int pageSize);
        Task<Course> GetCourse(string courseId);
        Task<Course> AddCourse(Course course);
        Task<bool> UpdateCourse(Course course);

    }
}
