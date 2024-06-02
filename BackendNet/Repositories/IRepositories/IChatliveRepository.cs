using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repository.IRepositories;

namespace BackendNet.Repositories.IRepositories
{
    public interface IChatliveRepository : IRepository<ChatLive>
    {
        Task<Chat> AddChatToChatLive(Chat chat, string roomId);
    }
}
