using AutoMapper;
using BackendNet.Dtos.Comment;
using BackendNet.Models;
using BackendNet.Repositories.IRepositories;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MongoDB.Driver;

namespace BackendNet.Services
{
    public class CommentService : ICommentService
    {
        private readonly IMapper _mapper;
        private readonly ICommentRepository _commentRepository;
        private readonly IUserService _userService;
        public CommentService(
            IMapper mapper
            , ICommentRepository commentRepository
            , IUserService userService)
        {
            _mapper = mapper;
            _commentRepository = commentRepository;
            _userService = userService;

        }
        public async Task<Comment> AddComment(CommentCreateDto commentDto, string userId, string module)
        {
            Comment cmt = null;

            _mapper.Map(commentDto, cmt);

            try
            {
                cmt.CDate = DateTime.UtcNow;
                cmt.Module = module;
                cmt.Like = 0;
                cmt.Dislike = 0;
                cmt.SubUser = await _userService.GetSubUser(userId);
                cmt = await _commentRepository.Add(cmt);

                return cmt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> DeleteComment(string commentId)
        {
            try
            {
                return await _commentRepository.RemoveByKey(nameof(Comment.Id), commentId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<PaginationModel<Comment>> GetComments(string module, string moduleId, int page, int pageSize)
        {
            var filterDef = Builders<Comment>.Filter.And(
                Builders<Comment>.Filter.Eq(x => x.ModuleId, moduleId),
                Builders<Comment>.Filter.Eq(x => x.Module, module)
            );
            var sortDef = Builders<Comment>.Sort.Descending(x => x.CDate);
            return await _commentRepository.GetManyByFilter(page, pageSize, filterDef, sortDef);
        }
    }
}
