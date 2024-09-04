using Amazon.Runtime.Internal;
using BackendNet.Controllers;
using BackendNet.Dtos.HubDto.Stream;
using BackendNet.Hubs;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.SignalR;
using SharpCompress.Common;

namespace BackendNet.Services
{
    public class StreamService : IStreamService
    {
        private readonly IUserService _userService;
        private readonly IConfiguration conf;
        private readonly IHubContext<RoomHub> _hubContext;
        private readonly string folderName;

        public StreamService(IUserService userService,IHubContext<RoomHub> hubContext,
                            IConfiguration configuration)
        {
            _userService = userService;
            _hubContext = hubContext;
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
                if (user.StreamInfo.Status == StreamStatus.Idle.ToString())
                {
                    _ = removeStreamVideo(streamKey);
                    string message = "200";
                   // _ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                }
                else
                {
                    await _userService.UpdateStreamStatusAsync(user.Id!, StreamStatus.Idle.ToString());
                    string message = streamKey;
                    //_ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                }
            }
            catch (Exception)
            {
                _ = removeStreamVideo(streamKey);
                string message = "200";
               // _ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                throw;
            }

        }

        public async Task<bool> onPublish(string requestBody)
        {
            string streamKey = getStreamKey(requestBody);
            var user = await _userService.GetUserByStreamKey(streamKey);

            if (user.CurrentActivity != null || user.StreamInfo == null || user.StreamInfo.Status == StreamStatus.Streaming.ToString())
                return false;
            StreamVideoUrlDto videoUrlDto = new StreamVideoUrlDto();

            videoUrlDto.videoUrl = Path.Combine(conf.GetValue<string>("NginxRtmpServer") ?? "", streamKey);
            videoUrlDto.waitTime = 5;
            _ = _hubContext.Clients.Group(user.CurrentActivity.value).SendAsync("OnStartStreaming", videoUrlDto);
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
