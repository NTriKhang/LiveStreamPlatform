using BackendNet.Models;
using BackendNet.Services;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IVideoService _videoService;
        private readonly IAwsService _awsService;
        public CourseController(
            ICourseService courseService
            , IVideoService videoService
            , IAwsService awsService
        )
        {
            _courseService = courseService;
            _videoService = videoService;
            _awsService = awsService;
        }
        [HttpGet]
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
        [HttpGet("{courseId}")]
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
        [HttpGet("getPresignedUrl")]
        public List<string> getPresignedUrl()
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
        //[HttpPost]
        //public async Task<ActionResult> postCourse([FromBody] Course course)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}
