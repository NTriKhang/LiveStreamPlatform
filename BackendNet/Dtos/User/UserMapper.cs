
using AutoMapper;
using BackendNet.Models;

namespace BackendNet.Dtos.User
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserProfileDto, Users>();
            CreateMap<Users, UserProfileDto>();

            CreateMap<UserUpdateDto, Users>();
            CreateMap<Users, UserUpdateDto>();
        }
    }
}
