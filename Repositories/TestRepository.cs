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

        /// <summary>
        /// Получить тест по ID со связанными вопросами и вариантами ответов.
        /// Переопределяем, т.к. нужно Include
        /// </summary>
        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _ctx.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                    .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Получить все тесты, подгрузив вопросы.
        /// </summary>
        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _ctx.Tests
                .Include(t => t.TestQuestions)
                    .ThenInclude(tq => tq.Question)
                .ToListAsync();
        }

        // Остальные CRUD-методы (Add, Update, Delete) 
        // мы используем из базового GenericRepository.
    }
}
