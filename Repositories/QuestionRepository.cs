using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        private readonly QuizDbContext _ctx;

        public QuestionRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<Question>> GetAllQuestionsAsync()
        {
            return await _ctx.Questions
                .Include(q => q.AnswerOptions) // Подгружаем варианты ответов
                .Include(q => q.Topic)        // Подгружаем Topic (если нужно)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(int id)
        {
            return await _ctx.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<List<Question>> GetQuestionsWithAnswersByIdsAsync(List<int> questionIds)
        {
            return await _ctx.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();
        }

        public async Task<List<Question>> GetRandomQuestionsAsync(int count)
        {
            return await _ctx.Questions
                .Include(q => q.AnswerOptions)
                .OrderBy(_ => Guid.NewGuid()) // простой способ рандомизировать
                .Take(count)
                .ToListAsync();
        }

        // Перегрузка, если надо ещё и по Topic
        public async Task<List<Question>> GetRandomQuestionsAsync(int count, int? topicId)
        {
            var query = _ctx.Questions
                .Include(q => q.AnswerOptions)
                .AsQueryable();

            if (topicId.HasValue)
            {
                query = query.Where(q => q.TopicId == topicId.Value);
            }

            return await query
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }
    }
}
