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
        Task<Rooms> GetActiveRoomByStreamKey(string streamKey);
        Task<IEnumerable<Rooms>> GetRoomByUserId(string userId);

        Task<ReturnModel> AddRoom(Rooms room);
        Task<bool> AddStudentToRoom(string roomId, SubUser student);

        Task<ReplaceOneResult> UpdateRoom(Rooms room);

        Task<bool> DeleteRoom(string roomId);

        Task SendRequestToTeacher(Rooms rooms, SubUser subUser, string cmd);
        Task ResponseRequestToStudent(ResponseRoomRequestDto response);
        Task<bool> RemoveFromRoom(RemoveFromRoomDto removeFrom);

        Task<bool> IsRoomHasUserId(string roomKey, string userId);
        Task SendChat(ChatLive chatLive);
        Task<PaginationModel<ChatLive>> GetChatsPagination(string roomId, int page, int pageSize);

    }
}
