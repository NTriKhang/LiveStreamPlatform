using AutoMapper;
using BackendNet.Dtos.Course;

namespace BackendNet.Dtos.HubDto.Room
{
    public class RoomMapper : Profile
    {
        public RoomMapper()
        {
            CreateMap<Models.ChatLive, ChatDto>();
            CreateMap<ChatDto, Models.ChatLive>();
        }
    }
}
