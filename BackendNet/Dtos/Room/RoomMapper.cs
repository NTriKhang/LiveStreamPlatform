using AutoMapper;
using BackendNet.Models;

namespace BackendNet.Dtos.Room
{
    public class RoomMapper : Profile
    {
        public RoomMapper()
        {
            CreateMap<RoomCreateDto, Rooms>();
            CreateMap<Rooms, RoomCreateDto>();
        }
    }
}
