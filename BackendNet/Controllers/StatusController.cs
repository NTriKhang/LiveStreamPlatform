using BackendNet.Models;
using BackendNet.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService statusService;
        public StatusController(IStatusService statusService)
        {
            this.statusService = statusService;
        }
        /// <summary>
        /// ControllerCode là tên của controller, vd: get status của room thì ControllerCode = Room, video thì ControllerCode = Video
        /// </summary>
        /// <param name="ControllerCode"></param>
        /// <returns></returns>
        [HttpGet("{ControllerCode}")]
        public async Task<IEnumerable<Status>> GetStatus(string ControllerCode)
        {
            return await statusService.GetStatus(ControllerCode);
        }

        /// <summary>
        /// Fe không xài api này
        /// </summary>
        /// <param name="StatusId"></param>
        /// <returns></returns>
        [HttpDelete("{StatusId}")]
        public async Task<ActionResult> DeleteStatus(string StatusId)
        {
            var res = await statusService.DeleteStatus(StatusId);
            return NoContent();
        }
        /// <summary>
        /// Fe không xài api này
        /// </summary>
        /// <param name="StatusId"></param>
        [HttpPost]
        public async Task<Status> PostStatus(Status status)
        {
            return await statusService.AddStatus(status);
        }
        /// <summary>
        /// Fe không xài api này
        /// </summary>
        /// <param name="StatusId"></param>
        [HttpPut]
        public async Task<ActionResult> UpdateStatus(Status status)
        {
            await statusService.ReplaceStatus(status);
            return NoContent();
        }
    }
}
