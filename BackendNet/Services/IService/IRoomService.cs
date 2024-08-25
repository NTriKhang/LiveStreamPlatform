using BackendNet.Models;
using MongoDB.Driver;

namespace BackendNet.Services.IService
{
    public interface IRoomService
    {
        Task<Rooms> GetRoomByRoomKey(string roomKey);
        Task<Rooms> GetRoomById(string roomId);
        Task<Rooms> AddRoom(Rooms room);
        Task<UpdateResult> UpdateRoomStatus(int status, string roomId);
        Task<bool> DeleteRoom(string roomId);
    }
}
