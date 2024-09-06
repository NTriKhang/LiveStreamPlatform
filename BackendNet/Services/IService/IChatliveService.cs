using BackendNet.Models;
using BackendNet.Setting;

namespace BackendNet.Services.IService
{
    public interface IChatliveService
    {
        Task<ChatLive> AddChat(ChatLive chat);
        Task<PaginationModel<ChatLive>> GetChatsPagination(string roomId, int page, int pageSize);
    }
}
