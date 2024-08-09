using AutoMapper;
using BackendNet.Dtos.Course;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Services;
using BackendNet.Services.IService;
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
        public async Task<IEnumerable<Course>> getCourses(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Course)
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
        [HttpGet("GetCoursePresignedUrl")]
        public List<string> getCoursePresignedUrl()
        {
            try
            {
                List<string> n = new List<string>();
                for(int i =0; i< 5; i++)
                {
                    string videoId = _videoService.GetIdYet();
                    n.Add(_awsService.GenerateVideoPostPresignedUrl(videoId,0));
                }
                return n;
            }   
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPut]
        [Authorize]
        public async Task<ActionResult> putCourse([FromBody] CourseCreateDto courseCreateDto)
        {
            try
            {
                Course crs = _mapper.Map<Course>(courseCreateDto);
                
                var subUser = new SubUser(
                        User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                        User?.FindFirstValue(ClaimTypes.Name) ?? "",
                        User?.FindFirstValue(ClaimTypes.UserData) ?? ""
                    );
                if (subUser.user_id == "")
                    return BadRequest("User is not valid");
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
                var subUser = new SubUser(
                        User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
                        User?.FindFirstValue(ClaimTypes.Name) ?? "",
                        User?.FindFirstValue(ClaimTypes.UserData) ?? ""
                    );
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
    }
}
