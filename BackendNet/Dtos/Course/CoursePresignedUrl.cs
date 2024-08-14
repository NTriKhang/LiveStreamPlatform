namespace BackendNet.Dtos.Course
{
    public class CoursePresignedUrl
    {
        public string url { get; set; }
        public string videoId { get; set; }
        public CoursePresignedUrl(string url, string videoId)
        {
            this.url = url;
            this.videoId = videoId;
        }
    }
}
