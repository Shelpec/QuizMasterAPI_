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
            if (string.IsNullOrWhiteSpace(questionDto.Text))
            {
                throw new ArgumentException("Question text cannot be empty.");
            }

            if (questionDto.AnswerOptions.Count != 3)
            {
                throw new ArgumentException("Question must have exactly 3 answer options.");
            }

            var question = new Question
            {
                Text = questionDto.Text,
                AnswerOptions = questionDto.AnswerOptions
                    .Select(a => new AnswerOption { Text = a.Text, IsCorrect = a.IsCorrect })
                    .ToList()
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return question;
        }

        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            var question = await GetQuestion(id);

            question.Text = questionDto.Text;
            _context.AnswerOptions.RemoveRange(question.AnswerOptions);

            question.AnswerOptions = questionDto.AnswerOptions.Select(a => new AnswerOption
            {
                Text = a.Text,
                IsCorrect = a.IsCorrect,
                QuestionId = id
            }).ToList();

            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<bool> DeleteQuestion(int id)
        {
            var question = await GetQuestion(id);
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckAnswer(int questionId, int selectedAnswerId)
        {
            var question = await GetQuestion(questionId);
            var selectedAnswer = question.AnswerOptions.FirstOrDefault(a => a.Id == selectedAnswerId);

            if (selectedAnswer == null)
            {
                throw new ArgumentException("Answer option not found.");
            }

            return selectedAnswer.IsCorrect;
        }

        //public async Task<int> StartQuiz(int questionCount, List<int> selectedAnswers)
        //{
        //    // Получаем вопросы
        //    var questions = await _context.Questions
        //        .Include(q => q.AnswerOptions)
        //        .Take(questionCount)
        //        .ToListAsync();

        //    // Если вопросов меньше, чем нужно
        //    if (questions.Count < questionCount)
        //    {
        //        throw new InvalidOperationException("Not enough questions available in the database.");
        //    }

        //    int correctAnswers = 0;

        //    // Проверяем каждый ответ
        //    for (int i = 0; i < questionCount; i++)
        //    {
        //        var question = questions[i];
        //        var selectedAnswerId = selectedAnswers[i];

        //        if (question.CorrectAnswerId == selectedAnswerId)
        //        {
        //            correctAnswers++;
        //        }
        //    }

        //    return correctAnswers;
        //}

    }
}
