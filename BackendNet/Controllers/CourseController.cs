using AutoMapper;
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
        private readonly IMapper _mapper;
        public CourseController(
            ICourseService courseService
            , IUserService userService
            , IVideoService videoService
            , IAwsService awsService
            , IMapper mapper
        )
        {
            _courseService = courseService;
            _userService = userService;
            _videoService = videoService;
            _awsService = awsService;
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
        [HttpGet("GetUserCourses/{userId}")]
        public async Task<PaginationModel<Course>> getCourses(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
        {
            try
            {
                return await _courseService.GetCourses(userId,page,pageSize);
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
                crs.Cuser = subUser;
                crs.Cdate = crs.Edate = DateTime.Now;

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
        [HttpPut("PutCourseVideo/{courseId}")]
        [Authorize]
        public async Task<ActionResult> PutCourseVideo(string courseId, [FromBody] VideoCreateDto videoCreate)
        {
            try
            {
                Videos video = _mapper.Map<VideoCreateDto, Videos>(videoCreate);
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
