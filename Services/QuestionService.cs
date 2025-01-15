using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly QuizDbContext _context;

        public QuestionService(QuizDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            return await _context.Questions
                .Include(q => q.AnswerOptions)
                .ToListAsync();
        }

        public async Task<Question> GetQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                throw new KeyNotFoundException($"Question with ID {id} not found.");
            }

            return question;
        }

        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            if (questionDto == null)
            {
                throw new ArgumentException("Question data cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(questionDto.Text))
            {
                throw new ArgumentException("Question text cannot be empty.");
            }

            if (questionDto.AnswerOptions == null || questionDto.AnswerOptions.Count != 3)
            {
                throw new ArgumentException("Question must have exactly 3 answer options.");
            }

            if (!questionDto.AnswerOptions.Any(a => a.Id == questionDto.CorrectAnswerId))
            {
                throw new ArgumentException("Correct answer ID must match one of the provided answer options.");
            }

            var question = new Question
            {
                Text = questionDto.Text,
                CorrectAnswerId = questionDto.CorrectAnswerId,
                AnswerOptions = questionDto.AnswerOptions
                    .Select(a => new AnswerOption { Text = a.Text })
                    .ToList()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return question;
        }

        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            var existingQuestion = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuestion == null)
            {
                throw new KeyNotFoundException($"Question with ID {id} not found.");
            }

            if (string.IsNullOrWhiteSpace(questionDto.Text))
            {
                throw new ArgumentException("Question text cannot be empty.");
            }

            if (questionDto.AnswerOptions == null || questionDto.AnswerOptions.Count != 3)
            {
                throw new ArgumentException("Question must have exactly 3 answer options.");
            }

            if (!questionDto.AnswerOptions.Any(a => a.Id == questionDto.CorrectAnswerId))
            {
                throw new ArgumentException("Correct answer ID must match one of the provided answer options.");
            }


            // Обновление текста и правильного ответа
            existingQuestion.Text = questionDto.Text;
            existingQuestion.CorrectAnswerId = questionDto.CorrectAnswerId;

            _context.AnswerOptions.RemoveRange(existingQuestion.AnswerOptions);

            // Добавляем новые варианты ответов
            existingQuestion.AnswerOptions = questionDto.AnswerOptions.Select(dto => new AnswerOption
            {
                Text = dto.Text
            }).ToList();

            await _context.SaveChangesAsync();
            return existingQuestion;
        }


        public async Task<bool> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                throw new KeyNotFoundException($"Question with ID {id} not found.");
            }

            _context.AnswerOptions.RemoveRange(question.AnswerOptions);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckAnswer(int questionId, int selectedAnswerId)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                throw new KeyNotFoundException($"Question with ID {questionId} not found.");
            }

            return question.CorrectAnswerId == selectedAnswerId;
        }
    }
}
