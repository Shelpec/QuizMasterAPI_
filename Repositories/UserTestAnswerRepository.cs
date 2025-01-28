using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class UserTestAnswerRepository : GenericRepository<UserTestAnswer>, IUserTestAnswerRepository
    {
        private readonly QuizDbContext _ctx;

        public UserTestAnswerRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<UserTestAnswer>> GetAnswersByUserTestQuestionId(int userTestQuestionId)
        {
            return await _ctx.UserTestAnswers
                .Where(a => a.UserTestQuestionId == userTestQuestionId)
                .ToListAsync();
        }
    }
}
