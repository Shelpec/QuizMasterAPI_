using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class UserTestService : IUserTestService
    {
        private readonly IUserTestRepository _userTestRepository;
        private readonly ITestRepository _testRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuizDbContext _context;
        private readonly ILogger<UserTestService> _logger;
        private readonly IMapper _mapper; // <-- AutoMapper

        public UserTestService(
            IUserTestRepository userTestRepository,
            ITestRepository testRepository,
            IQuestionRepository questionRepository,
            QuizDbContext context,
            ILogger<UserTestService> logger,
            IMapper mapper) // <-- получаем из DI
        {
            _userTestRepository = userTestRepository;
            _testRepository = testRepository;
            _questionRepository = questionRepository;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<UserTest> CreateAsync(UserTest userTest)
        {
            _logger.LogInformation("CreateAsync(UserId={UserId}, TestId={TestId})", userTest.UserId, userTest.TestId);
            try
            {
                await _userTestRepository.AddAsync(userTest);
                await _userTestRepository.SaveChangesAsync();
                return userTest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CreateAsync(UserId={UserId}, TestId={TestId})", userTest.UserId, userTest.TestId);
                throw;
            }
        }

        public async Task<UserTest?> GetByIdAsync(int id)
        {
            _logger.LogInformation("GetByIdAsync(UserTestId={Id})", id);
            try
            {
                return await _userTestRepository.GetUserTestAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetByIdAsync(UserTestId={Id})", id);
                throw;
            }
        }

        public async Task UpdateAsync(UserTest userTest)
        {
            _logger.LogInformation("UpdateAsync(UserTestId={Id})", userTest.Id);
            try
            {
                await _userTestRepository.UpdateAsync(userTest);
                await _userTestRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в UpdateAsync(UserTestId={Id})", userTest.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("DeleteAsync(UserTestId={Id})", id);
            try
            {
                var entity = await _userTestRepository.GetUserTestAsync(id);
                if (entity == null)
                    throw new KeyNotFoundException($"UserTest with id={id} not found");

                await _userTestRepository.DeleteAsync(entity);
                await _userTestRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в DeleteAsync(UserTestId={Id})", id);
                throw;
            }
        }

        public async Task<UserTestDto> StartTestAsync(int testId, string userId)
        {
            _logger.LogInformation("StartTestAsync(TestId={TestId}, UserId={UserId})", testId, userId);
            try
            {
                // 1) Шаблон теста
                var template = await _testRepository.GetTestByIdAsync(testId);
                if (template == null)
                    throw new KeyNotFoundException($"Тест-шаблон с ID={testId} не найден.");

                var count = template.CountOfQuestions;
                var topicId = template.TopicId;

                // 2) Рандомные вопросы
                var questions = await _questionRepository.GetRandomQuestionsAsync(count, topicId);

                // 3) Создаём UserTest
                var userTest = new UserTest
                {
                    UserId = userId,
                    TestId = template.Id,
                    TotalQuestions = questions.Count,
                    CorrectAnswers = 0,
                    IsPassed = false,
                    DateCreated = DateTime.UtcNow
                };

                await _userTestRepository.AddAsync(userTest);
                await _userTestRepository.SaveChangesAsync();

                var userTestQuestions = questions.Select(q => new UserTestQuestion
                {
                    UserTestId = userTest.Id,
                    QuestionId = q.Id
                }).ToList();

                _context.UserTestQuestions.AddRange(userTestQuestions);
                await _context.SaveChangesAsync();

                // Грузим созданный UserTest + Questions + AnswerOptions
                var createdUserTest = await _context.UserTests
                    .Include(ut => ut.UserTestQuestions)
                        .ThenInclude(utq => utq.Question)
                            .ThenInclude(q => q.AnswerOptions)
                    .FirstOrDefaultAsync(ut => ut.Id == userTest.Id);

                if (createdUserTest == null)
                    throw new Exception("Не удалось загрузить созданный UserTest.");

                // 4) Маппим в UserTestDto
                // Благодаря настройке в MappingProfile, 
                // userTestQuestions[].answerOptions будет заполняться из question.AnswerOptions
                var dto = _mapper.Map<UserTestDto>(createdUserTest);

                _logger.LogInformation("Тест успешно создан: UserTestId={UserTestId}", dto.Id);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в StartTestAsync(TestId={TestId}, UserId={UserId})", testId, userId);
                throw;
            }
        }
    }
}
