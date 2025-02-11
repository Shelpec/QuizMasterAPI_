using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class TestRepository : GenericRepository<Test>, ITestRepository
    {
        private readonly QuizDbContext _ctx;

        public TestRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _ctx.Tests
                .Include(t => t.Topic)
                .Include(t => t.TestQuestions) // Загружаем привязанные вопросы
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.AnswerOptions) // Загружаем варианты ответов
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _ctx.Tests
                .Include(t => t.Topic)
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.AnswerOptions) 
                .ToListAsync();
        }

        public async Task<List<Question>> GetTestQuestionsAsync(int testId)
        {
            var test = await _ctx.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                        .ThenInclude(q => q.AnswerOptions) 
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null || !test.TestQuestions.Any())
                return new List<Question>();

            return test.TestQuestions.Select(tq => tq.Question).ToList();
        }


    }
}
