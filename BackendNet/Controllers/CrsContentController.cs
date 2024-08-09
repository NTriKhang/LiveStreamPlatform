using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrsContentController : ControllerBase
    {
        private readonly ICrsContentService crsContentService;
        public CrsContentController(ICrsContentService crsContentService)
        {
            this.crsContentService = crsContentService;
        }
    }
}
