﻿using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface IVideoService
    {
        Task<Videos> AddVideoAsync(Videos video, IFormFile thumbnail);
        Task<Videos> AddVideoAsync(Videos video);
        Task<Videos> GetVideoAsync(string videoId);
        Task<IEnumerable<Videos>> GetVideos(string userId, int page);
        Task<IEnumerable<Videos>> GetFollowingVideos(string userId, int page);
        Task UpdateVideoStatus(int status, string id);
        Task UpdateVideoView(string videoId);
        Task<bool> RemoveVideo(string Id);


    }
}
