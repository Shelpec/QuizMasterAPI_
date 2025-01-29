using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class UserTestAnswerService : IUserTestAnswerService
    {
        private readonly IUserTestAnswerRepository _userTestAnswerRepo;
        private readonly IUserTestRepository _userTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuizDbContext _context;
        private readonly ILogger<UserTestAnswerService> _logger;
        private readonly IMapper _mapper;

        public UserTestAnswerService(
            IUserTestAnswerRepository userTestAnswerRepo,
            IUserTestRepository userTestRepository,
            IQuestionRepository questionRepository,
            QuizDbContext context,
            ILogger<UserTestAnswerService> logger,
            IMapper mapper)
        {
            _userTestAnswerRepo = userTestAnswerRepo;
            _userTestRepository = userTestRepository;
            _questionRepository = questionRepository;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task SaveAnswersAsync(int userTestId, List<UserAnswerSubmitDto> answers, string userId)
        {
            _logger.LogInformation("SaveAnswersAsync(UserTestId={Id}, CountAnswers={Count})", userTestId, answers.Count);
            try
            {
                var userTest = await _userTestRepository.GetUserTestWithQuestionsAsync(userTestId);
                if (userTest == null)
                    throw new KeyNotFoundException($"UserTest with ID={userTestId} not found");

                if (userTest.UserId != userId)
                    throw new UnauthorizedAccessException("Этот UserTest принадлежит другому пользователю.");

                foreach (var dto in answers)
                {
                    var utq = userTest.UserTestQuestions
                        .FirstOrDefault(x => x.Id == dto.UserTestQuestionId);
                    if (utq == null)
                        continue;

                    // Очищаем предыдущие ответы
                    utq.UserTestAnswers.Clear();

                    // Добавляем новые
                    foreach (var answerOptionId in dto.SelectedAnswerOptionIds)
                    {
                        var userTestAnswer = new UserTestAnswer
                        {
                            UserTestQuestionId = utq.Id,
                            AnswerOptionId = answerOptionId
                        };
                        await _userTestAnswerRepo.AddAsync(userTestAnswer);
                    }
                }

                await _userTestAnswerRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в SaveAnswersAsync(UserTestId={Id})", userTestId);
                throw;
            }
        }

        public async Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId)
        {
            _logger.LogInformation("CheckAnswersAsync(UserTestId={Id})", userTestId);
            try
            {
                var userTest = await _userTestRepository.GetUserTestWithQuestionsAsync(userTestId);
                if (userTest == null)
                    throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

                if (userTest.UserId != userId)
                    throw new UnauthorizedAccessException("Not your test.");

                var questionIds = userTest.UserTestQuestions.Select(q => q.QuestionId).ToList();
                var questions = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(questionIds);

                var result = new TestCheckResultDto
                {
                    TotalQuestions = userTest.TotalQuestions,
                    CorrectCount = 0,
                    Results = new List<QuestionCheckResultDto>()
                };

                int correctCount = 0;

                foreach (var utq in userTest.UserTestQuestions)
                {
                    var question = questions.FirstOrDefault(q => q.Id == utq.QuestionId);
                    if (question == null) continue;

                    var userChosen = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();
                    var correctOptions = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    bool isCorrect = !correctOptions.Except(userChosen).Any()
                                     && !userChosen.Except(correctOptions).Any();
                    if (isCorrect)
                        correctCount++;

                    var detail = new QuestionCheckResultDto
                    {
                        QuestionId = question.Id,
                        IsCorrect = isCorrect,
                        CorrectAnswers = question.AnswerOptions
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Text)
                            .ToList(),
                        SelectedAnswers = question.AnswerOptions
                            .Where(a => userChosen.Contains(a.Id))
                            .Select(a => a.Text)
                            .ToList()
                    };
                    result.Results.Add(detail);
                }

                result.CorrectCount = correctCount;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CheckAnswersAsync(UserTestId={Id})", userTestId);
                throw;
            }
        }
    }
}
