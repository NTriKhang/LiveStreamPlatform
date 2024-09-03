using BackendNet.Dtos.HubDto.Room;
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
        Task<IEnumerable<Rooms>> GetRoomByUserId(string userId);
        Task<ReturnModel> AddRoom(Rooms room);
        Task<ReplaceOneResult> UpdateRoom(Rooms room);
        Task<bool> DeleteRoom(string roomId);
        Task<bool> AddStudentToRoom(string roomId, SubUser student);
        Task SendRequestToTeacher(Rooms rooms, SubUser subUser, string cmd);
        Task ResponseRequestToStudent(ResponseRoomRequestDto response);
        Task<bool> RemoveStudentFromRoom(string roomId, string studentId);
    }
}
