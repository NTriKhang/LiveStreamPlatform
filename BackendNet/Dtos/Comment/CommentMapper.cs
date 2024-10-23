using AutoMapper;
using BackendNet.Dtos.Course;

namespace BackendNet.Dtos.Comment
{
    public class CommentMapper : Profile
    {
        public CommentMapper()
        {
            CreateMap<Models.Comment, CommentCreateDto>();
            CreateMap<CommentCreateDto, Models.Comment>();
        }
    }
}
