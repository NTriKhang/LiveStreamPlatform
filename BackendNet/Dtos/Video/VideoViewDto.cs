using BackendNet.Models;
using BackendNet.Models.Submodel;

namespace BackendNet.Dtos.Video
{
    public class VideoViewDto
    {
        public string? Id { get; set; }
        public SubUser User { get; set; }
        public DateTime? Time { set; get; }
        public string Title { set; get; }
        public string? Description { set; get; }
        public int? View { set; get; }
        public int? Like { set; get; }
        public string Thumbnail { set; get; }
        public string? Status { set; get; }
        public int? StatusNum { set; get; }
        public List<string> Tags { set; get; }
        public string FileType { set; get; }
        public string VideoUrl { set; get; }
        public VideoViewDto(Videos video, SubUser videoOwner, string videoUrl)
        {
            Id = video.Id;
            User = videoOwner;
            Time = video.Time;
            Title = video.Title;
            Description = video.Description;
            View = video.View;
            Like = video.Like;
            Thumbnail = video.Thumbnail;
            Status = video.Status;
            StatusNum = video.StatusNum;
            Tags = video.Tags;
            FileType = video.FileType;
            VideoUrl = videoUrl;
        }
    }
}
