using Amazon.Runtime.Internal;
using BackendNet.Controllers;
using BackendNet.Dtos.HubDto.Stream;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using Microsoft.AspNetCore.SignalR;
using SharpCompress.Common;
using ZstdSharp.Unsafe;

namespace BackendNet.Services
{
    public class StreamService : IStreamService
    {
        const string OnStreamingEvent = "OnStreaming";
        const string OnStopStreamEvent = "OnStopStream";

        private readonly IUserService _userService;
        private readonly IRoomService _roomService;
        private readonly IStatusService _statusService;
        private readonly IConfiguration conf;
        private readonly IHubContext<RoomHub> _hubContext;
        private readonly IHubContext<EduNimoHub> _eduNimoHub;
        private readonly string folderName;

        public StreamService(IUserService userService
            , IRoomService roomService
            , IStatusService statusService
            , IHubContext<RoomHub> hubContext
            , IHubContext<EduNimoHub> eduNimoHUb
            , IConfiguration configuration)
        {
            _userService = userService;
            _hubContext = hubContext;
            _roomService = roomService;
            _eduNimoHub = eduNimoHUb;
            _statusService = statusService;
            conf = configuration;
            folderName = configuration.GetValue<string>("FilePath")!;

        }

        public string getStreamKey(string requestbody)
        {
            var splitBody = requestbody.Split('&').ToList();
            var keyStream = splitBody.Where(x => x.StartsWith("name")).SingleOrDefault();
            string keyStreamValue = "";
            if (keyStream != null)
                keyStreamValue = keyStream.Split('=')[1];
            return keyStreamValue;
        }

        public async Task onPublishDone(string requestBody)
        {
            string streamKey = getStreamKey(requestBody);
            try
            {
                Users user = await _userService.GetUserByStreamKey(streamKey);
                if (user == null)
                    return;

                var res = await _userService.UpdateStreamStatusAsync(user.Id, StreamStatus.Idle.ToString());
                if (res.ModifiedCount == 0)
                {
                    var retunModel = new ReturnModel(400, "Lỗi hệ thống, file video sẽ bị mất hoặc bạn có thể tải file video record xuống", new { videoKey = streamKey });
                    await _eduNimoHub.Clients.Group(user.Id).SendAsync(OnStopStreamEvent, retunModel);
                }
                else
                {
                    var roomStatus = _statusService.GetStatus("Room");
                    var videoStatus = _statusService.GetStatus("Video");

                    await Task.WhenAll(roomStatus, videoStatus);
                    
                    var retunRoomModel = new ReturnModel(200, "Thao tác kết thúc", 
                        new
                        {
                            room = new { desc = "Thao tác với room", roomStatus.Result},
                            video = new {desc = "Thao tác với video", videoStatus.Result}
                        }    
                    );

                    await _eduNimoHub.Clients.Group(user.Id).SendAsync(OnStopStreamEvent, retunRoomModel);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> onPublish(string requestBody)
        {
            string streamKey = getStreamKey(requestBody);
            var user = await _userService.GetUserByStreamKey(streamKey);

            if (user == null || user.CurrentActivity == null || user.StreamInfo == null || user.StreamInfo.Status == StreamStatus.Streaming.ToString())
                return false;
            StreamVideoUrlDto videoUrlDto = new StreamVideoUrlDto();

            videoUrlDto.videoUrl = Path.Combine(conf.GetValue<string>("NginxRtmpServer") ?? "", streamKey, "index.m3u8");
            videoUrlDto.waitTime = 5;
            _ = _hubContext.Clients.Group(user.CurrentActivity.value).SendAsync(OnStreamingEvent, videoUrlDto);

            _ = Task.Run(async () =>
            {
                var room = await _roomService.GetRoomByRoomKey(streamKey);
                room.VideoUrl = videoUrlDto.videoUrl;
                await _roomService.UpdateRoom(room);
            });
            return true;
        }
        public async Task removeStreamVideo(string streamKey)
        {
            var filePaths = Directory.GetFiles(folderName, streamKey + '*').ToList();
            filePaths.AddRange(Directory.GetDirectories(folderName, streamKey + '*'));
            foreach (var filePath in filePaths)
            {
                if (filePath.EndsWith(".m3u8"))
                    File.Delete(filePath);
                else
                    Directory.Delete(filePath, true);
            }
            await Task.CompletedTask;
        }
    }
}
