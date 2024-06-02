using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;
using BackendNet.Repository.IRepositories;
using MongoDB.Driver;

namespace BackendNet.Repositories
{
    public class ChatliveRepository : Repository<ChatLive>, IChatliveRepository
    {
        public ChatliveRepository(IMongoContext context) : base(context)
        {

        }

        public Task<Chat> AddChatToChatLive(Chat chat, string roomId)
        {
            var filter = FilterId(nameof(ChatLive.room_id), roomId);
            //var update = Builders<ChatLive>.Update.AddToSet(x => x.chat, chat);
            throw new NotImplementedException();
        }
    }
}
