using BackendNet.Models;
using BackendNet.Repositories;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BackendNet.Services
{
    public class RecommendService : IRecommendService
    {
        private readonly IRecommendRepository trainModelRepository;
        public RecommendService(
            IRecommendRepository trainModelRepository    
        )
        {
            this.trainModelRepository = trainModelRepository;
        }
        public async Task UpdateRecommendInfo(string userId, string videoId)
        {
            try
            {
                var filDef = Builders<Recommend>.Filter.And(
                Builders<Recommend>.Filter.Eq(x => x.Id, userId),
                Builders<Recommend>.Filter.ElemMatch
                (
                    x => x.TrainInfoModel,
                    Builders<Interactions>.Filter.Eq(x => x.videoId, videoId)
                ));

                var recommend = await trainModelRepository.GetByFilter(filDef);
                if(recommend != null)
                {
                    var upDef = Builders<Recommend>.Update.Inc(x => x.TrainInfoModel.FirstMatchingElement().playTime, 1);
                    await trainModelRepository.UpdateByFilter(filDef, upDef);
                }
                else if (recommend == null)
                {
                    var newModel = new Recommend()
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
            catch (Exception)
            {

                throw;
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
