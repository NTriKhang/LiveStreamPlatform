using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;

namespace BackendNet.Services
{
    public class CrsContentService : ICrsContentService
    {
        private readonly ICrsContentRepository _contentRepository;
        public CrsContentService(ICrsContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }
    }
}
