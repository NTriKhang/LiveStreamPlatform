using AutoMapper;
using BackendNet.Dtos;
using BackendNet.Dtos.Comment;
using BackendNet.Dtos.Video;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using static BackendNet.Controllers.UserController;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IConfiguration _configuration;
        private readonly IAwsService _awsService;
        private readonly IUserService _userService;
        private readonly IFollowService _followService;
        private readonly ICommentService _commentService;
        private readonly IHistoryService _historyService;
        public VideoController(IVideoService videoService
            , IAwsService awsService
            , IConfiguration configuration
            , IUserService userService
            , IFollowService followService
            , ICommentService commentService
            , IHistoryService historyService

            )
        {
            _userService = userService;
            _videoService = videoService;
            _configuration = configuration;
            _awsService = awsService;
            _followService = followService;
            _commentService = commentService;
            _historyService = historyService;
        }
        //[HttpGet("findVideos/{title}")]
        //public async Task<PaginationModel<VideoViewDto>> FindVideos(string title)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        [HttpGet("getVideo/{videoId}")]
        public async Task<ActionResult<VideoViewDto>> GetVideo(string videoId)
        {
            try
            {
                string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

                var video = await _videoService.GetVideoAsync(videoId);
                if (video == null)
                    return NotFound();
                var subuser = await _userService.GetSubUser(video.User_id);
                string videoUrl = _configuration.GetValue<string>("CloudFrontEduVideo") ?? "";
                videoUrl += "/" + video.VideoUrl;

                if(video.FileType != string.Empty && video.FileType == "hls")
                {
                    videoUrl += "/index.m3u8";
                }

                VideoViewDto videoViewDto = new VideoViewDto(video, subuser, videoUrl);

                videoViewDto.Subscribe = await _followService.GetTotalFollow(video.User_id);

                if(userId != string.Empty)
                {
                    _ = Task.Run(async () =>
                    {
                        await _historyService.PostHistory(userId, videoId);
                    });
                }


                return StatusCode(StatusCodes.Status200OK, videoViewDto);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="uploadVideoDto"></param>
        /// <returns></returns>
        [HttpPost("getPresignedUrl")]
        [Authorize]
        public async Task<ActionResult> getPresignedUrl([FromBody] VideoCreateDto uploadVideoDto)
        {
            try
            {
                string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                string videoId = _videoService.GetAvailableId();

                var video = await _videoService.AddVideoAsync(new Videos
                {
                    Id = videoId,
                    Title = uploadVideoDto.title,
                    User_id = user_id,
                    StatusNum = (int)VideoStatus.Upload,
                    Description = uploadVideoDto.description ?? "",
                    Like = 0,
                    Time = DateTime.Now,
                    View = 0,
                    Thumbnail = uploadVideoDto.image_url ?? "https://t4.ftcdn.net/jpg/04/73/25/49/360_F_473254957_bxG9yf4ly7OBO5I0O5KABlN930GwaMQz.jpg",
                    Tags = uploadVideoDto.tags,
                    VideoSize = uploadVideoDto.video_size,
                    FileType = uploadVideoDto.file_type,
                    VideoUrl = videoId
                });
                if (video == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi không thể lưu thông tin video");
                string preSignedUrl = _awsService.GenerateVideoPostPresignedUrl(video.Id, uploadVideoDto.video_size);
                return Ok(new { preSignedUrl, video });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpPut("updateVideoStatus/{videoId}")]
        // [Authorize]
        public async Task<ActionResult> UpdateVideoStatus(string videoId, int status)
        {
            try
            {
                await _videoService.UpdateVideoStatus(status, videoId);
                //string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
                //if (status == (int)VideoStatus.Public)
                //{
                //    var emails = await _followService.GetFollowerEmail(userId); 
                //    if(emails != null)
                //    {
                //        _ = _emailService.SendMultiEmail(new Dtos.Mail.MultiMailRequest(
                //            $"Thông báo video mới",
                //            $"Khang vừa đăng tải video mới tại ...",
                //            emails.Select(x => x.AsString).ToList()
                //        ));
                //    }
                //}
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        [HttpDelete("deleteVideo/{videoId}")]
        public async Task<ActionResult> Delete(string videoId, [FromQuery] bool uploaded = false)
        {
            if (uploaded)
            {
                _ = _awsService.DeleteVideo(videoId);
            }
            bool res = await _videoService.RemoveVideo(videoId);
            if (res == false)
                return BadRequest(videoId);
            return NoContent();
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Videos>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                PaginationModel<Videos> listVideo = null;

                if(userId == string.Empty)
                    listVideo = await _videoService.GetNewestVideo(page, pageSize);
                else
                {
                    var historyFilter = Builders<History>.Filter.Eq(x => x.User.user_id, userId);
                    var historySort = Builders<History>.Sort.Descending(x => x.Time);
                    var recentHistoryData = await _historyService.GetHistoryByFilter(1, 100, historyFilter, historySort);

                    if(recentHistoryData == null)
                    {
                        listVideo = await _videoService.GetNewestVideo(page, pageSize);
                    }
                    else
                    {
                        var recentVideoIds = recentHistoryData.data.Select(doc => doc.Video.video_id).ToList();
                        listVideo = await _videoService.GetRecommendVideo(page, pageSize, recentVideoIds);
                    }
                }

                if (listVideo == null)
                    return StatusCode(StatusCodes.Status204NoContent, listVideo);

                List<VideoViewDto> videoView = new List<VideoViewDto>();
                foreach (var video in listVideo.data)
                {
                    string videoUrl = _configuration.GetValue<string>("CloudFrontEduVideo") ?? "";
                    videoUrl += "/" + video.VideoUrl;
                    var subUser = await _userService.GetSubUser(video.User_id);

                    VideoViewDto videoViewDto = new VideoViewDto(video, subUser, videoUrl);
                    videoView.Add(videoViewDto);
                }
                var paginationModel = new PaginationModel<VideoViewDto>()
                {
                    total_pages = listVideo.total_pages,
                    total_rows = listVideo.total_rows,
                    page = listVideo.page,
                    pageSize = listVideo.pageSize,
                    data = videoView
                };

                return StatusCode(StatusCodes.Status200OK, paginationModel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpDelete("DeleteS3Video/{videoId}")]
        public async Task<ActionResult> DeleteS3Video(string videoId)
        {
            try
            {
                await _awsService.DeleteVideo(videoId);
                return NoContent();
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("GetVideoOfCurrentUser")]
        [Authorize]
        public async Task<ActionResult<PaginationModel<Videos>>> GetVideoOfCurrentUser([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _videoService.GetUserVideos(page, pageSize, userId);
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
                return await _commentService.GetComments("Video", moduleId, page, pageSize);
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
                    , "Video"
                );
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[HttpGet("following/{page}")]
        //[Authorize]
        //public async Task<ActionResult<List<Videos>>> GetFollowingVideos(int page)
        //{
        //    try
        //    {
        //        string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        //        var listVideo = await _videoService.GetUserVideos(user_id, page);

        //        if (listVideo == null)
        //            return StatusCode(StatusCodes.Status204NoContent, listVideo);

        //        //string imageApi = _configuration.GetValue<string>("ImageApiGateWay")!;
        //        string videoThumbnail = "https://www.techsmith.com/blog/wp-content/uploads/2021/02/video-thumbnails-hero-1.png";

        //        //listVideo.ToList().ForEach(video => { video.Thumbnail = imageApi + video.Thumbnail; });
        //        listVideo.ToList().ForEach(video => { video.Thumbnail = videoThumbnail; });

        //        return StatusCode(StatusCodes.Status200OK, listVideo);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

    }
}
