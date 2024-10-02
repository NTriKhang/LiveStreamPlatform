using AutoMapper;
using BackendNet.Dtos;
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
        private readonly IRecommendService _recommendService;
        private readonly ITrendingService _trendingService;
        public VideoController(IVideoService videoService
            , IAwsService awsService
            , IConfiguration configuration
            , IUserService userService
            , IRecommendService recommendService
            , ITrendingService trendingService
            )
        {
            _userService = userService;
            _videoService = videoService;
            _configuration = configuration;
            _awsService = awsService;
            _recommendService = recommendService;
            _trendingService = trendingService;
        }
        //[HttpGet("delete/{streamKey}")]
        //public void DeleteVideo(string streamKey)
        //{
        //    _videoService.removeStreamVideo(streamKey);
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
                VideoViewDto videoViewDto = new VideoViewDto(video, subuser, videoUrl);

                _ = Task.Run(async () =>
                {
                    await _videoService.UpdateVideoView(videoId);
                    if (userId != string.Empty)
                    {
                        await _recommendService.UpdateRecommendInfo(userId, videoId);
                    }
                    await _trendingService.ProcessTrend(videoId, (int)videoViewDto.View + 1);
                });


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

        //[HttpPost("stream_video")]
        //[Authorize]
        //public async Task<HttpStatusCode> Stream_video([FromBody] UploadVideoDto uploadVideoDto)
        //{
        //    try
        //    {
        //        string? videoStreamingId = Request.Cookies["VideoStreamingId"];

        //        if(videoStreamingId == null) 
        //            return HttpStatusCode.BadRequest;

        //        if (uploadVideoDto.option == VideoStatus.Upload.ToString())
        //        {
        //            await _awsService.UploadStreamVideo(uploadVideoDto.streamKey, videoStreamingId);
        //            await _videoService.UpdateVideoStatus(VideoStatus.Public.ToString(), videoStreamingId);
        //        }
        //        else if(uploadVideoDto.option == VideoStatus.Remove.ToString())
        //        {
        //            await _videoService.UpdateVideoStatus(VideoStatus.Private.ToString(), videoStreamingId);
        //            await _streamService.removeStreamVideo(uploadVideoDto.streamKey);
        //        }
        //        string? roomKey = Request.Cookies["RoomKey"];
        //        await _roomService.UpdateRoomStatus(RoomStatus.Expired.ToString(), roomKey);                
        //        Response.Cookies.Delete("VideoStreamingId");
        //        Response.Cookies.Delete("RoomKey");
        //        return HttpStatusCode.NoContent;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
        [HttpGet]
        public async Task<ActionResult<List<Videos>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            try
            {
                var listVideo = await _videoService.GetNewestVideo(page, pageSize);

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

        /// <summary>
        /// Đăng nhập trước khi sử dụng
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetRecommendVideos")]
        [Authorize]
        public async Task<ActionResult<PaginationModel<Videos>>> GetRecommendVideos([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

            var res = await _videoService.GetRecommendVideo(page, pageSize, userId);
            return StatusCode(StatusCodes.Status200OK, res);
        }
        /// <summary>
        /// spam api getVideo/{videoId} để tăng GrowthRate trong collection Trending để video đc xem là trend trước
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetTrendingVideo")]
        public async Task<ActionResult<PaginationModel<Videos>>> GetTrendingVideo([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            return await _trendingService.GetTrendingVideo(page, pageSize);
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
