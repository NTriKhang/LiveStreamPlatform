using AutoMapper;
using BackendNet.Models;

namespace BackendNet.Dtos.Video
{
    public class VideoMapper : Profile
    {
        public VideoMapper()
        {
            CreateMap<VideoCreateDto, Videos>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.image_url))
                .ForMember(dest => dest.VideoSize, opt => opt.MapFrom(src => src.video_size))
                .ForMember(dest => dest.FileType, opt => opt.MapFrom(src => src.file_type))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.tags))
                .ForMember(dest => dest.Like, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.View, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StatusNum, opt => opt.MapFrom(src => src.status))
                .ForMember(dest => dest.Time, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
