using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;

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

        public Task<IEnumerable<ChatLive>> GetChatsPagination(int page)
        {
            throw new NotImplementedException();
        }
    }
}
