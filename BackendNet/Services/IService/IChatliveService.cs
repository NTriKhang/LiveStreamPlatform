using BackendNet.Models;

namespace BackendNet.Services.IService
{
    public interface IChatliveService
    {
        Task<ChatLive> AddChat(ChatLive chat);
        Task<IEnumerable<ChatLive>> GetChatsPagination(string roomId, int page);
    }
}
