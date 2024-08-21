﻿using AutoMapper;
using BackendNet.Dtos.Course;
using BackendNet.Dtos.Video;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Services;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IUserService _userService;
        private readonly IVideoService _videoService;
        private readonly IAwsService _awsService;
        private readonly IPaymenService _paymentService;
        private readonly IMapper _mapper;
        public CourseController(
            ICourseService courseService
            , IUserService userService
            , IVideoService videoService
            , IAwsService awsService
            , IPaymenService paymentService
            , IMapper mapper
        )
        {
            _courseService = courseService;
            _userService = userService;
            _videoService = videoService;
            _awsService = awsService;
            _paymentService = paymentService;
            _mapper = mapper;
        }
        //[HttpGet("GetUserCourse")]
        //public async Task<IEnumerable<Course>> getCourse(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
        //{
        //    try
        //    {
        //        return await _courseService.GetCourses(userId, page, pageSize);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        /// <summary>
        /// Chưa xài
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        [HttpPost("BuyCourse")]
        [Authorize]
        public async Task<ActionResult> BuyCourse(Course course)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var user = await _userService.GetUserById(userId);

            string sessionUrl = await _paymentService.CreatePaymentInfo(course, userId);
            Response.Headers["Location"] = sessionUrl;

            return StatusCode(StatusCodes.Status307TemporaryRedirect);
        }
        /// <summary>
        /// Get course mới nhất
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetNewestCourses")]
        public async Task<PaginationModel<Course>> getNewestCourse([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
        {
            try
            {
                return await _courseService.GetNewestCourses(page, pageSize);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("GetUserCourses/{userId}")]
        public async Task<PaginationModel<Course>> getCourses(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
        {
            try
            {
                return await _courseService.GetUserCourses(userId,page,pageSize);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet("GetCourse/{courseId}")]
        public async Task<Course> getCourse(string courseId)
        {
            try
            {
                return await _courseService.GetCourse(courseId);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("GetCourse")]
        public async Task<PaginationModel<Course>> getCourse([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "" ;
                return await _courseService.GetCourses(userId, page,pageSize);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// only use for test
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpDelete("{courseId}")]
        public async Task<ActionResult> deleteCourse(string courseId)
        {
            try
            {
                var res = await _courseService.DeleteCourse(courseId);
                if(res == false)
                    return BadRequest(res);
                return NoContent();
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// use in application
        /// </summary>
        /// <param name="courseCreateDto"></param>
        /// <returns></returns>
        [HttpDelete("CourseDelete/{courseId}")]
        [Authorize]
        public async Task<ActionResult> deleteCourseInUse(string courseId)
        {
            try
            {
                var course = await _courseService.GetCourse(courseId);
                if(course != null && course.Cuser.user_id == (User.FindFirstValue(ClaimTypes.NameIdentifier) ?? ""))
                {
                    var res = await _courseService.DeleteCourse(courseId);
                    if (res == false)
                        return BadRequest(res);
                    return NoContent();
                }
                return Unauthorized();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPut("{courseId}")]
        [Authorize]
        public async Task<ActionResult> putCourse(string courseId, [FromBody] CourseCreateDto courseCreateDto)
        {
            try
            {
                Course crs = _mapper.Map<Course>(courseCreateDto);

                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var subUser = await _userService.GetSubUser(userId);

                if (subUser.user_id == "")
                    return BadRequest("User is not valid");

                crs._id = courseId;
                crs.Edate = DateTime.Now;

                var res = await _courseService.UpdateCourse(crs);

                if(res)
                    return NoContent();
                return BadRequest("Nothing is updated");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> postCourse([FromBody] CourseCreateDto courseCreateDto)
        {
            try
            {
                Course crs = _mapper.Map<Course>(courseCreateDto);
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                var subUser = await _userService.GetSubUser(userId);

                if (subUser.user_id == "")
                    return BadRequest("User is not valid");

                crs.Cuser = subUser;
                crs.Cdate = crs.Edate = DateTime.Now;
                crs = await _courseService.AddCourse(crs);
                return CreatedAtAction("GetCourse", new { courseId = crs._id }, crs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
                Console.WriteLine(ex.StackTrace);
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// get url và videoId để đăng video vào course
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        [HttpGet("GetCoursePresignedUrl")]
        [Authorize]
        public List<CoursePresignedUrl> getCoursePresignedUrl([FromQuery] int n = 5)
        {
            try
            {
                List<CoursePresignedUrl> res = new List<CoursePresignedUrl>();
                for (int i = 0; i < (n > 5 ? 5 : n); i++)
                {
                    string videoId = _videoService.GetIdYet();
                    string url = _awsService.GenerateVideoPostPresignedUrl(videoId, 0);
                    res.Add(new CoursePresignedUrl(url,videoId));
                }
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Api để đăng video trong course
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="videoCreate"></param>
        /// <returns>Api để lưu thông video trong course vào database</returns>
        [HttpPut("PutCourseVideo/{courseId}")]
        [Authorize]
        public async Task<ActionResult> PutCourseVideo(string courseId, [FromBody] CourseVideoCreateDto videoCreate)
        {
            try
            {
                Videos video = _mapper.Map<CourseVideoCreateDto, Videos>(videoCreate);
                video.User_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                video.Id = videoCreate._id;
                var res = await _courseService.AddVideoToCrs(courseId, video);
                if(res.IsAcknowledged)
                    return NoContent();

                return BadRequest();
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpDelete("RemoveVideoFromCourse")]
        [Authorize]
        public async Task<ActionResult> RemoveVideoFromCourse([FromQuery] string courseId, [FromQuery] string videoId, [FromQuery] bool isUploaded = false)
        {
            try
            {
                if (courseId == null || videoId == null) 
                    return BadRequest();

                if (isUploaded)
                {
                    await _awsService.DeleteVideo(videoId);
                }

                var res = await _courseService.DeleteVideoFromCrs(courseId, videoId);
                if (res.IsAcknowledged)
                    return NoContent();

                return BadRequest();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
