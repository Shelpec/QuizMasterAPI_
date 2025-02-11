using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class TopicRepository : GenericRepository<Topic>, ITopicRepository
    {
        private readonly QuizDbContext _ctx;

        public TopicRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
        {
            // Если хотите, можете сделать .Include(t => t.Category) 
            // если нужно вытаскивать имя категории
            return await _ctx.Topics.ToListAsync();
        }

        public async Task<Topic?> GetTopicByIdAsync(int id)
        {
            return await _ctx.Topics.FindAsync(id);
        }
        public async Task<IEnumerable<Topic>> GetTopicsByCategoryIdAsync(int categoryId)
        {
            return await _ctx.Topics
                .Where(t => t.CategoryId == categoryId)
                .ToListAsync();
        }

    }
}
