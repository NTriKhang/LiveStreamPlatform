using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class TrendingService : ITrendingService
    {
        int ShortWindow = 2; // minutes
        int LongWindow = 30; // minutes
        private readonly ITrendingRepository _trendRepository;
        public TrendingService(
            ITrendingRepository trendingRepository
            )
        {
            _trendRepository = trendingRepository;
        }
        public async Task ProcessTrend(string videoId, int videoView)
        {
            var filterDef = Builders<Trending>.Filter.Eq(x => x.Id, videoId);
            var trend = await _trendRepository.GetByFilter(filterDef);
            if(trend == null)
            {
                var newTrend = new Trending()
                {
                    Id = videoId,
                    GrowthRate = 0,
                    NewestViewAt = DateTime.Now,
                    TotalView = videoView,
                    ViewTrackHistories = new List<Models.Submodel.ViewTrackHistory>()
                };
                newTrend.ViewTrackHistories.Add(new Models.Submodel.ViewTrackHistory()
                {
                    timeStamp = DateTime.Now,
                    views = 1
                });
                newTrend = await _trendRepository.Add(newTrend);
            }
            else
            {
                await CaculateGrowthRate(trend);
            }
        }
        private async Task CaculateGrowthRate(Trending trending)
        {
            if (trending.ViewTrackHistories[0].timeStamp.AddMinutes(ShortWindow) < DateTime.Now)
            {
                var newViewTrack = new ViewTrackHistory()
                {
                    timeStamp = DateTime.Now,
                    views = 0
                };
                trending.ViewTrackHistories.Insert(0, newViewTrack);
            }
            trending.TotalView++;
            trending.ViewTrackHistories[0].views++;

            double scalingFactor = trending.ViewTrackHistories.Count == 1 ? 0.1 : 1.0;

            int preView = 1;
            if(trending.ViewTrackHistories.Count > 1)
            {
                preView = trending.ViewTrackHistories[1].views;
            }
            trending.GrowthRate = scalingFactor * ((double)(trending.ViewTrackHistories[0].views * 100) / preView);
            var filterDef = Builders<Trending>.Filter.Eq(x => x.Id, trending.Id);
            await _trendRepository.ReplaceAsync(filterDef, trending);
        }
    }
}
