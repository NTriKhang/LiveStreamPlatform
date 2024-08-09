using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class ChatliveService : IChatliveService
    {
        private readonly IChatliveRepository _chatliveRepository;
        public ChatliveService(IChatliveRepository chatliveRepository)
        {

            _chatliveRepository = chatliveRepository;

        }
        public async Task<ChatLive> AddChat(ChatLive chat)
        {
            return await _chatliveRepository.Add(chat);
        }

        public async Task<IEnumerable<ChatLive>> GetChatsPagination(string roomId,int page)
        {
            SortDefinition<ChatLive> sort = Builders<ChatLive>.Sort.Descending(x => x.createdAt);
            return await _chatliveRepository.GetManyByKey(nameof(ChatLive.room_id), roomId, page, (int)PaginationCount.Chat,null, sort);
        }
    }
}
