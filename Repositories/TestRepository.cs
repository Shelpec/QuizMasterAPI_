using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class TestRepository : ITestRepository
    {
        private readonly QuizDbContext _context;

        public TestRepository(QuizDbContext context)
        {
            _context = context;
        }

        public async Task<Test> CreateTestAsync(Test test)
        {
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _context.Tests
                .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                .ThenInclude(q => q.AnswerOptions)  // <-- Подгружаем варианты ответов
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _context.Tests
                .Include(t => t.TestQuestions)
                .ThenInclude(tq => tq.Question)
                .ToListAsync();
        }
    }
}
