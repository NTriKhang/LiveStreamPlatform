using BackendNet.Dtos.Comment;
using BackendNet.Models;
using BackendNet.Setting;

namespace BackendNet.Services.IService
{
    public interface ICommentService
    {
        Task<PaginationModel<Comment>> GetComments(string module, string moduleId, int page, int pageSize);
        Task<Comment> AddComment(CommentCreateDto commentDto, string userId, string module);
        Task<bool> DeleteComment(string commentId);
    }
}
