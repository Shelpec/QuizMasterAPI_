using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class TestAccessRepository : GenericRepository<TestAccess>, ITestAccessRepository
    {
        private readonly QuizDbContext _ctx;

        public TestAccessRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<bool> HasAccessAsync(int testId, string userId)
        {
            return await _ctx.TestAccesses
                .AnyAsync(ta => ta.TestId == testId && ta.UserId == userId);
        }

        public async Task<List<TestAccess>> GetTestAccessListAsync(int testId)
        {
            return await _ctx.TestAccesses
                .Where(ta => ta.TestId == testId)
                .ToListAsync();
        }
    }
}
