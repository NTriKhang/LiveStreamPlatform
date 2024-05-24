using BackendNet.Models;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IRoomService
    {
        Task<Rooms> GetRoomByRoomKey(string roomKey);
        Task<Rooms> AddRoom(Rooms room);
        Task<UpdateResult> UpdateRoomStatus(string status, string roomKey);
    }
}
