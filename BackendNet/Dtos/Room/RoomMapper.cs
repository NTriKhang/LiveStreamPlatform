﻿using AutoMapper;
using BackendNet.Models;

namespace BackendNet.Dtos.Room
{
    public class RoomMapper : Profile
    {
        public RoomMapper()
        {
            CreateMap<RoomCreateDto, Rooms>();
            CreateMap<Rooms, RoomCreateDto>();

            CreateMap<RoomUpdateDto, Rooms>();
            CreateMap<Rooms,RoomUpdateDto>();

            CreateMap<Rooms, RoomViewDto>();
            CreateMap<RoomViewDto, Rooms>();
        }
    }
}
