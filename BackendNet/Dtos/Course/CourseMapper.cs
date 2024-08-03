using AutoMapper;
using BackendNet.Models;

namespace BackendNet.Dtos.Course
{
    public class CourseMapper : Profile
    {
        public CourseMapper()
        {
            CreateMap<Models.Course, CourseCreateDto>();
            CreateMap<CourseCreateDto, Models.Course>();
        }
    }
}
