using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class UserTestRepository : GenericRepository<UserTest>, IUserTestRepository
    {
        private readonly QuizDbContext _ctx;

        public UserTestRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<UserTest?> GetUserTestWithQuestionsAsync(int userTestId)
        {
            return await _ctx.UserTests
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(ut => ut.Id == userTestId);
        }

        public async Task<UserTest?> GetUserTestAsync(int userTestId)
        {
            return await _ctx.UserTests
                .FirstOrDefaultAsync(ut => ut.Id == userTestId);
        }
    }
}
