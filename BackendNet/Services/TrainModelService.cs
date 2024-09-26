using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Bson;
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
        public async Task<PaginationModel<Videos>> OrderByInteraction(int page, int pageSize)
        {
            var pipeline = new[]
            {
                new BsonDocument
                {
                    { "$limit", 10 }
                },
                new BsonDocument
                {
                    { "$unwind", "$Interactions" }
                },
                //new BsonDocument
                //{
                //    { "$group", new BsonDocument
                //        {
                //            { "_id", "$Interactions.videoId" },
                //            { "interactionCount", new BsonDocument { { "$sum", 1 } } }
                //        }
                //    }
                //},
                //new BsonDocument
                //{
                //    { "$sort", new BsonDocument { { "interactionCount", -1 } } }
                //}
            };
            var result = await trainModelRepository.ExecAggre(pipeline);
            if (result.Any())
            {
                var doc = result.First();
            }
            return null;

        }
    }
}
