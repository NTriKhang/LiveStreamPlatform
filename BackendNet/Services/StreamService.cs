using Amazon.Runtime.Internal;
using BackendNet.Controllers;
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
        private readonly IHubContext<StreamHub, IStreamHub> _hubContext;
        private readonly string folderName;

        public StreamService(IUserService userService,IHubContext<StreamHub,IStreamHub> hubContext,
                            IConfiguration configuration)
        {
            _userService = userService;
            _hubContext = hubContext;
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
                    _ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                }
                else
                {
                    await _userService.UpdateStreamStatusAsync(user.Id!, StreamStatus.Idle.ToString());
                    string message = streamKey;
                    _ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                }
            }
            catch (Exception)
            {
                _ = removeStreamVideo(streamKey);
                string message = "200";
                _ = _hubContext.Clients.Group(streamKey).OnStopStreaming(message);
                throw;
            }
             
        }

        public async Task<bool> onPublish(string requestBody)
        {
            string streamKey = getStreamKey(requestBody);
            Console.WriteLine("On publish" + streamKey);
            if ((await _userService.IsTokenExist(streamKey)) == false)
                return false;
            Console.WriteLine("On publish" + streamKey);
            string message = StreamStatus.Streaming.ToString();
            _ = _hubContext.Clients.Group(streamKey).OnStartStreaming(message);
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
