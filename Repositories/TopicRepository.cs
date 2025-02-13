using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Repositories;

public class TopicRepository : GenericRepository<Topic>, ITopicRepository
{
    private readonly QuizDbContext _ctx;

    public TopicRepository(QuizDbContext context) : base(context)
    {
        _ctx = context;
    }

    public async Task<IEnumerable<Topic>> GetAllTopicsAsync()
    {
        // Например, подгружаем Category, если нужно
        return await _ctx.Topics
            //.Include(t => t.Category) // если нужна инфа о категории
            .ToListAsync();
    }

    public async Task<Topic?> GetTopicByIdAsync(int id)
    {
        // То же самое, если нужно с категорией
        return await _ctx.Topics
            //.Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Topic>> GetTopicsByCategoryIdAsync(int categoryId)
    {
        return await _ctx.Topics
            //.Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();
    }
}
