﻿using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Setting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;

namespace BackendNet.Services.IService
{
    public interface ICourseService
    {
        Task<PaginationModel<Course>> GetNewestCourses(int page, int pageSize);
        Task<PaginationModel<Course>> GetUserCourses(string userId, int page, int pageSize);
        Task<PaginationModel<Course>> GetCourses(string userId, int page, int pageSize);
        Task<Course> GetCourse(string courseId);
        Task<Course> AddCourse(Course course);
        Task<bool> UpdateCourse(Course course);
        Task<bool> DeleteCourse(string courseId);
        Task<UpdateResult> AddVideoToCrs(string courseId, Videos videos);
        Task<UpdateResult> DeleteVideoFromCrs(string courseId, string videoId);
        Task<UpdateResult> BuyCourse(string courseId, CourseStudent courseStudent);

    }
}
