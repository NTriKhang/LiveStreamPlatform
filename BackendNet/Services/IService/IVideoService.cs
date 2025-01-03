﻿using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Setting;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Services.IService
{
    public interface IVideoService
    {
        Task<PaginationModel<Videos>> SearchVideo(
        int page
        , int pageSize
        , string title);

        Task<Videos> AddVideoAsync(Videos video);
        Task<Videos> GetVideoAsync(string videoId);
        Task<SubVideo> GetSubVideo(string videoId);
        Task<PaginationModel<Videos>> GetNewestVideo(int page, int pageSize);
        Task<PaginationModel<Videos>> GetRecommendVideo(int page, int pageSize, List<string> recentVideoIds);
        Task<PaginationModel<Videos>> GetUserVideos(int page, int pageSize, string userId);
        Task UpdateVideoStatus(int status, string id);
        Task UpdateVideoView(string videoId);
        Task UpdateLike(string videoId, bool isLike);
        Task<bool> RemoveVideo(string Id);
        string GetAvailableId();

    }
}
