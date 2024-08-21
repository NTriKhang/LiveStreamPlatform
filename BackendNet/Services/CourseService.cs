using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BackendNet.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository courseRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        public CourseService(
            ICourseRepository courseRepository
            , IHttpContextAccessor httpContextAccessor
        )
        {
            this.courseRepository = courseRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Course> AddCourse(Course course)
        {
            return await courseRepository.Add(course);
        }
        public async Task<bool> UpdateCourse(Course course)
        {
            var filter = Builders<Course>.Filter.Eq(x => x._id, course._id);
            var res = await courseRepository.ReplaceAsync(filter, course);
            if (res.IsAcknowledged)
                return true;
            return false;
        }
        public Task<IEnumerable<Course>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<Course> GetCourse(string courseId)
        {
            return await courseRepository.GetByKey(nameof(Course._id), courseId);
        }

        public async Task<PaginationModel<Course>> GetUserCourses(string userId, int page, int pageSize)
        {
            SortDefinition<Course> sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            return await courseRepository.GetManyByKey($"{nameof(Course.Cuser)}.{nameof(Course.Cuser.user_id)}", userId, page, pageSize, null, sort);
        }

        public async Task<bool> DeleteCourse(string courseId)
        {
            return await courseRepository.RemoveByKey(nameof(Course._id), courseId);
        }

        public async Task<UpdateResult> AddVideoToCrs(string courseId, Videos videos)
        {
            var updateDef = Builders<Course>.Update.Push(x => x.Videos, videos);
            return await courseRepository.UpdateByKey(nameof(Course._id), courseId, null, updateDef);
        }

        public async Task<UpdateResult> DeleteVideoFromCrs(string courseId, string videoId)
        {
            var updateDef = Builders<Course>.Update.PullFilter(s => s.Videos, f => f.Id == videoId);
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var addFilter = Builders<Course>.Filter.Eq(x => x.Cuser.user_id, userId);
            return await courseRepository.UpdateByKey(nameof(Course._id), courseId, addFilter, updateDef);
        }

        public async Task<PaginationModel<Course>> GetCourses(string userId, int page, int pageSize)
        {
            SortDefinition<Course> sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            var filter = Builders<Course>.Filter.ElemMatch(x => x.Students, o => o.user_id == userId);
            return await courseRepository.GetMany(page, pageSize, filter, sort);
        }

        public async Task<PaginationModel<Course>> GetNewestCourses(int page, int pageSize)
        {
            SortDefinition<Course> sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            //var filter = Builders<Videos>.Filter.Ne(u => u.StatusNum, (int)VideoStatus.TestData);

            return await courseRepository.GetMany(page, pageSize, null, sort);
        }
    }
}
