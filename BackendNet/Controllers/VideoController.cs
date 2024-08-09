using BackendNet.Dtos;
using BackendNet.Dtos.Video;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services;
using BackendNet.Services.IService;
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
        //private readonly IRoomService _roomService;
        private readonly IConfiguration _configuration;
        private readonly IAwsService _awsService;
        private readonly IEmailService _emailService;
        //private readonly IStreamService _streamService;
        private readonly IFollowService _followService;
        private readonly IUserService _userService;

        public VideoController(IVideoService videoService
                                , IAwsService awsService
                                , IFollowService followService
                                , IEmailService emailService
                                , IConfiguration configuration
                                , IUserService userService)
        {
            _userService = userService;
            _videoService = videoService;
            //_roomService = roomService;
            _configuration = configuration;
            _emailService = emailService;
            _awsService = awsService;
            _followService = followService;
            //_streamService = streamService;
        }
        //[HttpGet("delete/{streamKey}")]
        //public void DeleteVideo(string streamKey)
        //{
        //    _videoService.removeStreamVideo(streamKey);
        //}
        [HttpGet("getVideo/{videoId}")]
        public async Task<ActionResult<Videos>> GetVideo(string videoId)
        {
            try
            {
                return StatusCode(StatusCodes.Status200OK, await _videoService.GetVideoAsync(videoId));
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost("getPresignedUrl")]
        [Authorize]
        public async Task<ActionResult> getPresignedUrl([FromBody] OnPublicDto uploadVideoDto)
        {
            try
            {
                string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var video = await _videoService.AddVideoAsync(new Videos
                {
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
                    FileType = uploadVideoDto.file_type
                });
                if (video == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi không thể lưu thông tin video");
                string preSignedUrl = _awsService.GenerateVideoPostPresignedUrl(video.Id, uploadVideoDto.video_size);
                return Ok(new { preSignedUrl, video});
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }
        [HttpPut("updateVideoStatus/{videoId}")]
       // [Authorize]
        public async Task<ActionResult> UpdateVideoStatus(string videoId, int status)
        {
            try
            {
                //await _videoService.UpdateVideoStatus(status, videoId);

                if (status == (int)VideoStatus.Public)
                {
                    var emails = await _followService.GetFollowerEmail("6682131597bd9b8b7a61ac8f");
                    if(emails != null)
                    {
                        _ = _emailService.SendMultiEmail(new Dtos.Mail.MultiMailRequest(
                            $"Thông báo video mới",
                            $"Khang vừa đăng tải video mới tại ...",
                            emails.Select(x => x.AsString).ToList()
                        ));
                    }
                }
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
        //[Authorize]
        public async Task<ActionResult<List<Videos>>> GetVideos([FromQuery] int page = 1, [FromQuery] int pageSize = (int)PaginationCount.Video)
        {
            try
            {
                var listVideo = await _videoService.GetNewestVideo(page, pageSize);

                if (listVideo == null)
                    return StatusCode(StatusCodes.Status204NoContent, listVideo);

                IList<VideoViewDto> videoView = new List<VideoViewDto>();
                foreach (var video in listVideo)
                {
                    string videoUrl = _configuration.GetValue<string>("CloudFrontEduVideo") ?? "";
                    videoUrl += "/" + video.Id;
                    var subUser = await _userService.GetUserById(video.User_id);
                    if(subUser != null)
                        videoView.Add(new VideoViewDto
                            (
                                video, 
                                new Models.Submodel.SubUser
                                (
                                    subUser.Id,
                                    subUser.DislayName,
                                    subUser.AvatarUrl
                                ),
                                videoUrl
                            ));
                    else
                        videoView.Add(new VideoViewDto
                            (
                                video,
                                new Models.Submodel.SubUser
                                (
                                    "No user",
                                    "No user",
                                    "https://i0.wp.com/digitalhealthskills.com/wp-content/uploads/2022/11/3da39-no-user-image-icon-27.png?fit=500%2C500&ssl=1"
                                ),
                                videoUrl
                            ));
                }
                return StatusCode(StatusCodes.Status200OK, videoView);
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
        //[Authorize]
        //[HttpPost("on_stream")]
        //public async Task<ActionResult<Rooms>> PublicVideoStream([FromForm] string streamInfo)
        //{
        //    try
        //    {
        //        string? videoStreamingId = Request.Cookies["VideoStreamingId"];
        //        if (videoStreamingId != null)
        //            return StatusCode(StatusCodes.Status405MethodNotAllowed);

        //        var streamInfoDto = JsonConvert.DeserializeObject<OnPublicDto>(streamInfo);

        //        if (streamInfoDto == null)
        //            return StatusCode(StatusCodes.Status400BadRequest, default(Videos));

        //        var thumbnail = Request.Form.Files[0];

        //        string? streamKey = User.FindFirstValue(ClaimTypes.UserData);
        //        if (streamKey == null)
        //            return StatusCode(StatusCodes.Status401Unauthorized);
        //        string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        //        var video = await _videoService.AddVideoAsync(new Videos
        //        {
        //            Title = streamInfoDto.title,
        //            User_id = user_id,
        //            Status = VideoStatus.Keep.ToString(),
        //            Description = streamInfoDto.description ?? null,
        //            Like = 0,
        //            Time = DateTime.Now,
        //            View = 0
        //        }, thumbnail);
        //        if (video == null)
        //            return BadRequest("VIdeo is null");

        //        var room = await _roomService.AddRoom(new Rooms { RoomKey = Guid.NewGuid().ToString().Substring(0,32), Status = RoomStatus.Opening.ToString(),
        //                                        StreamKey = streamKey,Video = video  });
   
        //        Response.Cookies.Append("VideoStreamingId", video.Id);
        //        Response.Cookies.Append("RoomKey", room.RoomKey);
        //        return Ok(room);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return NotFound();
        //    }
        //}
        //[Authorize]
        //[HttpGet("generateKey")]
        //public async Task<ActionResult<string>> generateKey()
        //{
        //    try
        //    {
        //        string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //        if (user_id == null)
        //            return BadRequest();
        //        var key = _videoService.generateKey();
        //        var streamToken = key + Utitlity.SplitKeyStream + user_id;
        //        var userUpdate = await _userService.UpdateStreamTokenAsync(user_id, key, StreamTokenStatus.Init.ToString());
        //        return Ok(new StreamKeyDto() { Key = streamToken });
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

    }
}
