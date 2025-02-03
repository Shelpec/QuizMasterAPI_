// Repositories/TopicRepository.cs
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
            return await _ctx.Topics.ToListAsync();
        }

        public async Task<Topic?> GetTopicByIdAsync(int id)
        {
            return await _ctx.Topics.FindAsync(id);
        }
    }
}
