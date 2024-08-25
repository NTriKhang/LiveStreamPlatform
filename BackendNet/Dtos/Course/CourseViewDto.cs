using BackendNet.Models.Submodel;
using BackendNet.Models;
using BackendNet.Dtos.Video;

namespace BackendNet.Dtos.Course
{
    public class CourseViewDto
    {
        public string _id { set; get; } = string.Empty;
        public string Title { set; get; } = string.Empty;
        public string Desc { set; get; } = string.Empty;
        public string CourseDetail { set; get; } = string.Empty;
        public string CourseImage { set; get; } = string.Empty;
        public decimal Price { set; get; }
        public List<string> Tags { set; get; }
        public decimal Discount { set; get; }
        public SubUser Cuser { set; get; }
        public DateTime Cdate { set; get; }
        public DateTime Edate { set; get; }
        public List<CourseStudent> Students { set; get; }
        public List<VideoViewDto> Videos { set; get; }

    }
}
