using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly QuizDbContext _context;

        public QuestionRepository(QuizDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .ToListAsync();
        }

        public async Task<Question> GetQuestionByIdAsync(int id)
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _context.Questions.AddAsync(question);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(Question question)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Question>> GetQuestionsWithAnswersByIdsAsync(List<int> questionIds)
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();
        }

        public async Task<List<Question>> GetRandomQuestionsAsync(int count)
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .OrderBy(q => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }



    }

}
