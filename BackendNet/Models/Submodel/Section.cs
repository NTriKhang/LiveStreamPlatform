using BackendNet.Dtos.Video;

namespace BackendNet.Models.Submodel
{
    public class Section
    {
        public string title { get; set; }
        public List<VideoCourseDto> videos { get; set; }
    }
}
