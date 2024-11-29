using AutoMapper;
using BackendNet.Dtos.Comment;
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
        private readonly IStripeService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ICommentService _commentService;

        public CourseController(
            ICourseService courseService
            , IUserService userService
            , IVideoService videoService
            , IAwsService awsService
            , IStripeService paymentService
            , IConfiguration configuration
            , IMapper mapper
            , ICommentService commentService

        )
        {
            _courseService = courseService;
            _userService = userService;
            _videoService = videoService;
            _awsService = awsService;
            _paymentService = paymentService;
            _configuration = configuration;
            _mapper = mapper;
            _commentService = commentService;

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
        /// thông tin người dùng mua course là thông tin người đăng nhập
        /// </summary>
        /// <param name="course"></param>
        /// <returns></returns>
        [HttpPost("BuyCourse")]
        [Authorize]
        public async Task<ActionResult> BuyCourse(string courseId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var course = await _courseService.GetCourse(courseId);
            if (course.Students.Any(x => x.user_id == userId) || course.Cuser.user_id == userId)
                return StatusCode(StatusCodes.Status403Forbidden);

            var user = await _userService.GetUserById(userId);
            var resUrl = await _paymentService.CreatePaymentInfo(course, user);

            return Ok(resUrl);

        }
        [HttpGet("ConfirmCheckout")]
        public async Task<ActionResult> ConfirmCheckout([FromQuery] string userId, [FromQuery] string courseId)
        {
            var user = await _userService.GetSubUser(userId);
            if (user == null)
                return BadRequest();
            var res = await _courseService.BuyCourse(courseId, new CourseStudent(userId, user.user_name, user.user_avatar));
            if (res.ModifiedCount == 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            _ = Task.Run(async () =>
            {
                var crs = await _courseService.GetCourse(courseId);
                var subIncome = new SubTrade()
                {
                    CourseId = courseId,
                    CourseTitle = crs.Title,
                    CourseImage = crs.CourseImage,
                    Price = crs.Price,
                    DateIncome = DateTime.UtcNow
                };
                Task income = _userService.UpdateIncome(crs.Cuser.user_id, subIncome);
                Task outcome = _userService.UpdateOutcome(userId, subIncome);

                await Task.WhenAll(income, outcome);
            });

            return NoContent();
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
        /// <summary>
        /// Get các course của 1 tài khoản teacher
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get chi tiết 1 course
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [HttpGet("GetCourse/{courseId}")]
        [AllowAnonymous]
        public async Task<ActionResult<CourseViewDto>> getCourse(string courseId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

                var course = await _courseService.GetCourse(courseId);
                if (course == null)
                    return NoContent();
                var courseView = new CourseViewDto();
                _mapper.Map(course, courseView);
                courseView.Videos = new List<VideoViewDto>();

                bool isStudent = course.Students.Where(x => x.user_id ==  userId).Any();
                foreach(var video in course.Videos)
                {
                    string videoUrl = string.Empty;
                    if (video.StatusNum == (int)VideoStatus.Public
                        || userId == course.Cuser.user_id
                        || isStudent)
                    {
                        videoUrl = _configuration.GetValue<string>("CloudFrontEduVideo") ?? "";
                        videoUrl += "/" + video.VideoUrl;
                    }
                    courseView.Videos.Add(new VideoViewDto(video, course.Cuser, videoUrl));

                }

                return Ok(courseView);
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Get các course mà user đó đã mua
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetCourse")]
        [Authorize]
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
        /// Đăng nhập trước khi sử dụng
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
        /// <summary>
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="courseCreateDto"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="courseCreateDto"></param>
        /// <returns></returns>
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
                    string videoId = _videoService.GetAvailableId();
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
                video.VideoUrl = videoCreate._id;
                video.StatusNum = int.Parse(video.Status);
                video.Time = DateTime.UtcNow;
                video.View = 0;
                video.Like = 0;


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
        [HttpGet("GetComment")]
        public async Task<PaginationModel<Comment>> GetComment(
            [FromQuery] string moduleId
            , [FromQuery] int page = 1
            , [FromQuery] int pageSize = (int)PaginationCount.Course
        )
        {
            try
            {
                return await _commentService.GetComments("Course", moduleId, page, pageSize);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost("PostComment")]
        [Authorize]
        public async Task<Comment> PostComment(
            CommentCreateDto createDto
        )
        {
            try
            {
                var res = await _commentService.AddComment(
                    createDto
                    , User.FindFirstValue(ClaimTypes.NameIdentifier)
                    , "Course"
                );
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
