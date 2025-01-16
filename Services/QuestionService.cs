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

        public async Task<IEnumerable<QuestionDto>> GetRandomQuestions(int count)
        {
            var questions = await _context.Questions
                .Include(q => q.AnswerOptions)
                .OrderBy(q => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                HasMultipleCorrectAnswers = q.AnswerOptions.Count(a => a.IsCorrect) > 1,
                AnswerOptions = q.AnswerOptions.Select(a => new AnswerOptionDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            });
        }

        public async Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers)
        {
            var questionIds = answers.Select(a => a.QuestionId).ToList();
            var questions = await _context.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();

            var response = new AnswerValidationResponseDto();
            foreach (var answer in answers)
            {
                var question = questions.First(q => q.Id == answer.QuestionId);
                var correctAnswers = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                var isCorrect = !correctAnswers.Except(answer.SelectedAnswerIds).Any() &&
                                !answer.SelectedAnswerIds.Except(correctAnswers).Any();

                response.Results.Add(new QuestionValidationResultDto
                {
                    QuestionText = question.Text,
                    CorrectAnswers = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text)
                        .ToList(),
                    SelectedAnswers = question.AnswerOptions
                        .Where(a => answer.SelectedAnswerIds.Contains(a.Id))
                        .Select(a => a.Text)
                        .ToList()
                });

                if (isCorrect)
                {
                    response.CorrectCount++;
                }
            }

            return response;
        }

        //public async Task<IEnumerable<RandomQuestionDto>> GetRandomQuestionsAsync(int questionCount)
        //{
        //    var questions = await _context.Questions
        //        .Include(q => q.AnswerOptions) // Загружаем варианты ответов
        //        .OrderBy(q => Guid.NewGuid()) // Рандомизируем порядок
        //        .Take(questionCount) // Берем указанное количество
        //        .ToListAsync();

        //    return questions.Select(q => new RandomQuestionDto
        //    {
        //        QuestionId = q.Id,
        //        QuestionText = q.Text,
        //        AnswerOptions = q.AnswerOptions.Select(a => new AnswerOptionDto
        //        {
        //            Id = a.Id,
        //            Text = a.Text
        //        }).ToList()
        //    });
        //}


        //public async Task<IEnumerable<AnswerCheckResultDto>> CheckAnswersAsync(IEnumerable<AnswerSubmissionDto> answers)
        //{
        //    var results = new List<AnswerCheckResultDto>();

        //    foreach (var answer in answers)
        //    {
        //        var question = await _context.Questions
        //            .Include(q => q.AnswerOptions)
        //            .FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

        //        if (question == null)
        //        {
        //            continue; // Если вопрос не найден, пропускаем
        //        }

        //        var correctAnswer = question.AnswerOptions.FirstOrDefault(a => a.IsCorrect);
        //        var selectedAnswer = question.AnswerOptions.FirstOrDefault(a => a.Id == answer.SelectedAnswerId);

        //        results.Add(new AnswerCheckResultDto
        //        {
        //            QuestionText = question.Text,
        //            CorrectAnswerText = correctAnswer?.Text ?? "Unknown",
        //            SelectedAnswerText = selectedAnswer?.Text ?? "Unknown",
        //            IsCorrect = correctAnswer?.Id == selectedAnswer?.Id
        //        });
        //    }

        //    return results;
        //}



    }
}
