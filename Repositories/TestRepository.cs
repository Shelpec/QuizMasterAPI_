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
            // Если нужно подгружать Topic, добавляйте .Include(t => t.Topic)
            return await _ctx.Tests
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _ctx.Tests
                .ToListAsync();
        }
    }
}
