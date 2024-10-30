using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repository.IRepositories;
using MongoDB.Driver;

namespace BackendNet.Repositories.IRepositories
{
    public interface IRoomRepository : IRepository<Rooms>
    {
        Task<Rooms> AddRoom(Rooms room);
        Task<bool> AddStudentToRoom(string roomId, SubUser student);
        Task<bool> RemoveFromRoom(string roomId, string studentId);
        Task<bool> RemoveRoom(Rooms room);

    }
}
