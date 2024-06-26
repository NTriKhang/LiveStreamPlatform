﻿using BackendNet.Dtos;
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
        private readonly IRoomService _roomService;
        private readonly IConfiguration _configuration;
        private readonly IAwsService _awsService;
        private readonly IStreamService _streamService;
        private readonly IHubContext<StreamHub, IStreamHub> _hubContext;
        private readonly IChatliveService _chatliveService;

        public VideoController(IVideoService videoService, IRoomService roomService, 
            IConfiguration configuration, IAwsService awsService, IStreamService streamService,
            IHubContext<StreamHub, IStreamHub> hubContext, IChatliveService chatliveService)
        {
            _videoService = videoService;
            _roomService = roomService;
            _configuration = configuration;
            _awsService = awsService;
            _streamService = streamService;
            _hubContext = hubContext;
            _chatliveService = chatliveService;
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
        [HttpPost("upload_video")]
        [Authorize]
        public async Task<HttpStatusCode> Upload_video([FromBody] UploadVideoDto uploadVideoDto)
        {
            try
            {
                string? videoStreamingId = Request.Cookies["VideoStreamingId"];

                if(videoStreamingId == null) 
                    return HttpStatusCode.BadRequest;

                if (uploadVideoDto.option == VideoStatus.Upload.ToString())
                {
                    await _awsService.UploadStreamVideo(uploadVideoDto.streamKey, videoStreamingId);
                    await _videoService.UpdateVideoStatus(VideoStatus.Public.ToString(), videoStreamingId);
                }
                else if(uploadVideoDto.option == VideoStatus.Remove.ToString())
                {
                    await _videoService.UpdateVideoStatus(VideoStatus.Private.ToString(), videoStreamingId);
                    await _streamService.removeStreamVideo(uploadVideoDto.streamKey);
                }
                string? roomKey = Request.Cookies["RoomKey"];
                await _roomService.UpdateRoomStatus(RoomStatus.Expired.ToString(), roomKey);                
                Response.Cookies.Delete("VideoStreamingId");
                Response.Cookies.Delete("RoomKey");
                return HttpStatusCode.NoContent;
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet("{page}")]
        [Authorize]
        public async Task<ActionResult<List<Videos>>> GetVideos(int page)
        {
            try
            {
                string user_id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var listVideo = await _videoService.GetVideos(user_id, page);

                if (listVideo == null)
                    return StatusCode(StatusCodes.Status204NoContent, listVideo);

                string imageApi = _configuration.GetValue<string>("ImageApiGateWay")!;

                listVideo.ToList().ForEach(video => { video.Thumbnail = imageApi + video.Thumbnail; });
                return StatusCode(StatusCodes.Status200OK, listVideo);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [Authorize]
        [HttpPost("on_stream")]
        public async Task<ActionResult<Rooms>> PublicVideoStream([FromForm] string streamInfo)
        {
            try
            {
                string? videoStreamingId = Request.Cookies["VideoStreamingId"];
                if (videoStreamingId != null)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed);

                var streamInfoDto = JsonConvert.DeserializeObject<OnPublicDto>(streamInfo);

                if (streamInfoDto == null)
                    return StatusCode(StatusCodes.Status400BadRequest, default(Videos));

                var thumbnail = Request.Form.Files[0];

                string? streamKey = User.FindFirstValue(ClaimTypes.UserData);
                if (streamKey == null)
                    return StatusCode(StatusCodes.Status401Unauthorized);

                var video = await _videoService.AddVideoAsync(new Videos
                {
                    Title = streamInfoDto.title,
                    User_id = streamInfoDto.user_id,
                    Status = VideoStatus.Keep.ToString(),
                    Description = streamInfoDto.description ?? null,
                    Video_token = string.Empty,
                    Like = 0,
                    Time = DateTime.Now,
                    View = 0
                }, thumbnail);
                if (video == null)
                    return BadRequest("VIdeo is null");

                var room = await _roomService.AddRoom(new Rooms { RoomKey = Guid.NewGuid().ToString().Substring(0,32), Status = RoomStatus.Opening.ToString(),
                                                StreamKey = streamKey,Video = video  });
   
                Response.Cookies.Append("VideoStreamingId", video.Id);
                Response.Cookies.Append("RoomKey", room.RoomKey);
                return Ok(room);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return NotFound();
            }
        }
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
