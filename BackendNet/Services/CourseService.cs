﻿using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IEnumerable<Course>> GetCourses(string userId, int page, int pageSize)
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
            return await courseRepository.UpdateByKey(nameof(Course._id), courseId, updateDef);
        }

        public Task<UpdateResult> DeleteVideoFromCrs(string videoId)
        {
            throw new NotImplementedException();
        }
    }
}
