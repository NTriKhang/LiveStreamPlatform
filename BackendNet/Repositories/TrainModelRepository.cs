using BackendNet.DAL;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Repository;

namespace BackendNet.Repositories
{
    public class TrainModelRepository : Repository<WatchTrainModel>, ITrainModelRepository
    {
        public TrainModelRepository(IMongoContext context) : base(context)
        {
        }
    }
}
