using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizMasterAPI.Repositories
{
    public class TestQuestionRepository : GenericRepository<TestQuestion>, ITestQuestionRepository
    {
        private readonly QuizDbContext _ctx;

        public TestQuestionRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<IEnumerable<TestQuestion>> GetAllByTestIdAsync(int testId)
        {
            return await _ctx.TestQuestions
                .Where(tq => tq.TestId == testId)
                .Include(tq => tq.Question)
                .ToListAsync();
        }
    }
}
