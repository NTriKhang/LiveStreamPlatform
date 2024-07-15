namespace BackendNet.Dtos.Video
{
    public class OnPublicDto
    {
        public string title { set; get; }
        public string description { set; get; }
        public string image_url { set; get; } = string.Empty;
        public int? video_status { set; get; } = 0; // 0 là public, 1 là private
        public long video_size { set; get; }
        public string file_type { set; get; }   
        public List<string> tags { set; get; }  
    }
}
