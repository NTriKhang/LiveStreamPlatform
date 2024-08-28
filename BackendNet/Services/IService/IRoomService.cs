using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Setting;
using MongoDB.Driver;
using System.Data.SqlClient;

namespace BackendNet.Services.IService
{
    public interface IRoomService
    {
        Task<Rooms> GetRoomByRoomKey(string roomKey);
        Task<Rooms> GetRoomById(string roomId);
        Task<ReturnModel> AddRoom(Rooms room);
        Task<ReplaceOneResult> UpdateRoom(Rooms room);
        Task<bool> DeleteRoom(string roomId);
        Task<UpdateResult> AddStudentToRoom(string roomId, SubUser student);
        Task SendRequestToTeacher(Rooms rooms, SubUser subUser, string cmd);

    }
}
