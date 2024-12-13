using AutoMapper;
using BackendNet.Dtos.Room;
using BackendNet.Models;

namespace BackendNet.Dtos
{
    public class ChatLiveMapper : Profile
    {
        public ChatLiveMapper()
        {
            CreateMap<ChatDto, ChatLive>();
            CreateMap<ChatLive, ChatDto>();
        }
    }
}
