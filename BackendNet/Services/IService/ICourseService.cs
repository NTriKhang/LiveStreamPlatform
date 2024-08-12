using BackendNet.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;

namespace BackendNet.Services.IService
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAll();
        Task<IEnumerable<Course>> GetCourses(string userId, int page, int pageSize);
        Task<Course> GetCourse(string courseId);
        Task<Course> AddCourse(Course course);
        Task<bool> UpdateCourse(Course course);
        Task<bool> DeleteCourse(string courseId);
        Task<UpdateResult> AddVideoToCrs(string courseId, Videos videos);
        Task<UpdateResult> DeleteVideoFromCrs(string videoId);
    }
}
