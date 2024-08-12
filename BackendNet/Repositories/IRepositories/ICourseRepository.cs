using BackendNet.Models;
using BackendNet.Repository.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace BackendNet.Repositories.IRepositories
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<ActionResult> AddVideoToCrs(Videos videos);
        Task<ActionResult> DeleteVideoFromCrs(string videoId);
    }
}
