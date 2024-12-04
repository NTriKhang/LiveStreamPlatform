using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SysRelationshipController : ControllerBase
    {
        private readonly ISysRelationshipRepo sysRelationshipRepo; 
        public SysRelationshipController(ISysRelationshipRepo sysRelationshipRepo)
        {
            this.sysRelationshipRepo = sysRelationshipRepo; 
        }
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SysRelationship sysRelationship)
        {
            var res = await sysRelationshipRepo.Add(sysRelationship);
            return Ok(res);
        }
    }
}
