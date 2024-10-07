using BackendNet.Models;
using BackendNet.Models.Submodel;
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
        public async Task<UpdateResult> BuyCourse(string courseId, CourseStudent courseStudent)
        {
            var filter = Builders<Course>.Filter.Eq(x => x._id, courseId);
            var updateDef = Builders<Course>.Update.Push(x => x.Students, courseStudent);

            return await courseRepository.UpdateByFilter(filter, updateDef);
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
        public async Task<Course> GetCourse(string courseId)
        {
            var filter = Builders<Course>.Filter.Eq(x => x._id, courseId);

            return await courseRepository.GetByFilter(filter);
        }

        public async Task<PaginationModel<Course>> GetUserCourses(string userId, int page, int pageSize)
        {
            var filter = Builders<Course>.Filter.Eq(x => x.Cuser.user_id, userId);
            var sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            var proj = Builders<Course>.Projection.Exclude(x => x.Videos);

            return await courseRepository.GetManyByFilter(page, pageSize, filter, sort, proj);
        }

        public async Task<bool> DeleteCourse(string courseId)
        {
            return await courseRepository.RemoveByKey(nameof(Course._id), courseId);
        }

        public async Task<UpdateResult> AddVideoToCrs(string courseId, Videos videos)
        {
            var filterDef = Builders<Course>.Filter.Eq(x => x._id, courseId);
            var updateDef = Builders<Course>.Update.Push(x => x.Videos, videos);
            return await courseRepository.UpdateByFilter(filterDef, updateDef);
        }

        public async Task<UpdateResult> DeleteVideoFromCrs(string courseId, string videoId)
        {
            var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var filter = Builders<Course>.Filter.And(
                Builders<Course>.Filter.Eq(x => x._id, courseId),
                Builders<Course>.Filter.Eq(x => x.Cuser.user_id, userId) 
            );
            var updateDef = Builders<Course>.Update.PullFilter(s => s.Videos, f => f.Id == videoId);
            return await courseRepository.UpdateByFilter(filter, updateDef);
        }

        public async Task<PaginationModel<Course>> GetCourses(string userId, int page, int pageSize)
        {
            var sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            var filter = Builders<Course>.Filter.ElemMatch(x => x.Students, o => o.user_id == userId);
            var proj = Builders<Course>.Projection.Exclude(x => x.Videos);

            return await courseRepository.GetManyByFilter(page, pageSize, filter, sort, proj);
        }

        public async Task<PaginationModel<Course>> GetNewestCourses(int page, int pageSize)
        {
            var filter = Builders<Course>.Filter.Empty;
            var sort = Builders<Course>.Sort.Descending(x => x.Cdate);
            var proj = Builders<Course>.Projection.Exclude(x => x.Videos);
            //var filter = Builders<Videos>.Filter.Ne(u => u.StatusNum, (int)VideoStatus.TestData);

            return await courseRepository.GetManyByFilter(page, pageSize, filter, sort, proj);
        }
    }
}
