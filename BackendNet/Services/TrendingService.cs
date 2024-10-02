using BackendNet.Models;
using BackendNet.Models.Submodel;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class TrendingService : ITrendingService
    {
        int ShortWindow = 10; // hours
        private readonly ITrendingRepository _trendRepository;
        private readonly IVideoRepository _videoRepository;
        public TrendingService(
            ITrendingRepository trendingRepository
            , IVideoRepository videoRepository
        )
        {
            _trendRepository = trendingRepository;
            _videoRepository = videoRepository;
        }
        public async Task<PaginationModel<Videos>> GetTrendingVideo(int page, int pageSize)
        {
            int longWindow = 2; // days
            int threashold = 100;

            var filterWinthinLongWindow = Builders<Trending>.Filter.And(
                Builders<Trending>.Filter.Gt(x => x.NewestViewAt, DateTime.UtcNow.AddDays(-longWindow)),
                Builders<Trending>.Filter.Gt(x => x.GrowthRate, threashold)
            );
            var sortWithinLongWindow = Builders<Trending>.Sort.Descending(x => x.GrowthRate);
            var projDef = Builders<Trending>.Projection.Exclude(x => x.ViewTrackHistories);

            var trendingVideo = await _trendRepository.GetManyByFilter(page, pageSize, filterWinthinLongWindow, sortWithinLongWindow, projDef);
            if(trendingVideo.data == null)
            {
                return new PaginationModel<Videos>()
                {
                    data = null,
                    page = 1,
                    pageSize = 0,
                    total_pages = 0,
                    total_rows = 0,
                };
            }
            var growthRates = trendingVideo.data.ToDictionary(x => x.Id, x => x.GrowthRate);


            var filterVideo = Builders<Videos>.Filter.In(x => x.Id, trendingVideo.data.Select(x => x.Id));
            var res = await _videoRepository.GetManyByFilter(page, pageSize, filterVideo, null);

            res.data = res.data.OrderByDescending(x => growthRates[x.Id]).ToList();
            return res;
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
                    NewestViewAt = DateTime.UtcNow,
                    TotalView = videoView,
                    ViewTrackHistories = new List<ViewTrackHistory>()
                };
                newTrend.ViewTrackHistories.Add(new ViewTrackHistory()
                {
                    timeStamp = DateTime.UtcNow,
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
            if (trending.ViewTrackHistories[0].timeStamp.AddHours(ShortWindow) < DateTime.UtcNow)
            {
                var newViewTrack = new ViewTrackHistory()
                {
                    timeStamp = DateTime.UtcNow,
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

            trending.NewestViewAt = DateTime.UtcNow;
            await _trendRepository.ReplaceAsync(filterDef, trending);
        }
        
    }
}
