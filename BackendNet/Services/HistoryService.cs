using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IHIstoryRepository _historyRepository;
        private readonly IUserService _userService;
        private readonly IVideoService _videoService;
        public HistoryService(IHIstoryRepository hIstoryRepository
            , IUserService userService
            , IVideoService videoService
            )
        {
            _historyRepository = hIstoryRepository;
            _userService = userService;
            _videoService = videoService;
        }
        public async Task<PaginationModel<History>> GetHistoryByFilter(int page, int pageSize, FilterDefinition<History> filterDefinition, SortDefinition<History> sortDefinition)
        {
            return await _historyRepository.GetManyByFilter(page, pageSize, filterDefinition, sortDefinition);
        }
        public async Task<PaginationModel<History>> GetHistory(string userId, int page, int pageSize)
        {
            var filterDef = Builders<History>.Filter.Eq(x => x.User.user_id, userId);
            SortDefinition<History> sort = Builders<History>.Sort.Descending(x => x.Time);

            return await _historyRepository.GetManyByFilter(page, (int)pageSize, filterDef, sort, null);
        }
        public async Task<History> PostHistory(string userId, string videoId)
        {
            var filter = Builders<History>.Filter.And(
                Builders<History>.Filter.Eq(x => x.User.user_id, userId),
                Builders<History>.Filter.Eq(x => x.Video.video_id, videoId)
            );
            var history = await _historyRepository.GetByFilter(filter);
            if(history != null)
            {
                history.PlayTime++;
                history.Time = DateTime.UtcNow;

                await _historyRepository.ReplaceAsync(filter, history);
                return history;
            }

            var user = await _userService.GetSubUser(userId);
            var video = await _videoService.GetSubVideo(videoId);
            return await _historyRepository.Add(new History(
                    video
                    , user
                    , 1
                    , DateTime.UtcNow
                    , false
                    , false
                    , 0
                ));
        }
        public Task<bool> RemoveHistory(string Id)
        {
            throw new NotImplementedException();
        }
    }
}
