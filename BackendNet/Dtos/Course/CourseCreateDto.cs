namespace BackendNet.Dtos.Course
{
    public class CourseCreateDto
    {
        public string Title { set; get; } = string.Empty;
        public string Desc { set; get; } = string.Empty;
        public string CourseDetail { set; get; } = string.Empty;
        public decimal Price { set; get; }
        public List<string> Tags { set; get; }
        public decimal Discount { set; get; }
    }
}
