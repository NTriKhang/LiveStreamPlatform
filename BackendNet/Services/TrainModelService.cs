using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class TrainModelService : ITrainModelService
    {
        private readonly ITrainModelRepository trainModelRepository;
        public TrainModelService(
            ITrainModelRepository trainModelRepository    
        )
        {
            this.trainModelRepository = trainModelRepository;
        }
        public async Task UpdateInfo(string userId, string videoId)
        {
            var filDef = Builders<WatchTrainModel>.Filter.And(
                Builders<WatchTrainModel>.Filter.Eq(x => x.Id, userId),
                Builders<WatchTrainModel>.Filter.ElemMatch
                (
                    x => x.TrainInfoModel,
                    Builders<Interactions>.Filter.Eq(x => x.videoId,videoId)
                ));
            var upDef = Builders<WatchTrainModel>.Update.Inc(x => x.TrainInfoModel[-1].playTime, 1);

            var res = await trainModelRepository.FindOneAndUpdateAsync(filDef, upDef);  
            if(res == null)
            {
                var newModel = new WatchTrainModel()
                {
                    Id = userId,
                    TrainInfoModel = new List<Interactions>
                    {
                        new Interactions()
                        {
                            videoId = videoId,
                            playTime = 1
                        }
                    }
                };
                await trainModelRepository.Add(newModel);
            }
        }
    }
}
