namespace BackendNet.Dtos.Video
{
    public class VideoCreateDto
    {
        public string _id {  get; set; }
        public string title { set; get; }
        public string description { set; get; }
        public string image_url { set; get; } = string.Empty;
        public long video_size { set; get; }
        public string file_type { set; get; }
        public int status { set; get; } = (int)VideoStatus.Public;
        public List<string> tags { set; get; }
    }
}
