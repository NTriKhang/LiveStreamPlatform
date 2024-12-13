using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
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

        public async Task<PaginationModel<ChatLive>> GetChatsPagination(string roomId,int page, int pageSize)
        {
            var filterDef = Builders<ChatLive>.Filter.Eq(x => x.room_id, roomId);
            SortDefinition<ChatLive> sort = Builders<ChatLive>.Sort.Descending(x => x.createdAt);
            return await _chatliveRepository.GetManyByFilter(page, (int)pageSize, filterDef, sort);
        }
    }
}
