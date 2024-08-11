using BackendNet.Services;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrsContentController : ControllerBase
    {
        private readonly ICrsContentService _crsContentService;
        private readonly IAwsService _awsService;
        private readonly IVideoService _videoService;

        public CrsContentController(
            ICrsContentService crsContentService
            , IAwsService awsService
            , IVideoService videoService
        )
        {
            _crsContentService = crsContentService;
            _awsService = awsService;
            _videoService = videoService;
        }
        [HttpGet("GetCoursePresignedUrl")]
        [Authorize]
        public List<string> getCoursePresignedUrl()
        {
            try
            {
                List<string> n = new List<string>();
                for (int i = 0; i < 5; i++)
                {
                    string videoId = _videoService.GetIdYet();
                    n.Add(_awsService.GenerateVideoPostPresignedUrl(videoId, 0));
                }
                return n;
            }
            catch (Exception)
            {

                throw;
            }
        }
        //[HttpPost("PostSession")]
        //public Task<ActionResult> PostSession()
    }
}
