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
                dto.IsSurveyTopic = userTest.Test.IsSurvey;

                _logger.LogInformation("Тест успешно создан: UserTestId={UserTestId}", dto.Id);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в StartTestAsync(TestId={TestId}, UserId={UserId})", testId, userId);
                throw;
            }


        }

        // UserTestService.cs
        public async Task<UserTestHistoryDto?> GetFullUserTestAsync(int userTestId)
        {
            // 1) Грузим "полный" UserTest
            var userTest = await _userTestRepository.GetUserTestFullAsync(userTestId);
            if (userTest == null)
                return null;

            // 2) Считаем, сколько вопросов и сколько из них правильны
            var totalQuestions = userTest.UserTestQuestions.Count;
            int correctCount = 0;

            foreach (var utq in userTest.UserTestQuestions)
            {
                var chosenIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();
                var correctIds = utq.Question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                bool isCorrect = !correctIds.Except(chosenIds).Any() &&
                                 !chosenIds.Except(correctIds).Any();
                if (isCorrect)
                    correctCount++;
            }

            // 3) Собираем DTO
            var dto = new UserTestHistoryDto
            {
                // Основные поля UserTest
                UserTestId = userTest.Id,
                DateCreated = userTest.DateCreated,
                IsPassed = userTest.IsPassed,     // Или можно переопределить (isPassed = correctCount == totalQuestions)
                CorrectAnswers = correctCount,
                TotalQuestions = userTest.TotalQuestions,

                // Инфо о пользователе
                UserId = userTest.UserId,
                UserEmail = userTest.User?.Email,
                UserFullName = userTest.User?.FullName,

                // Инфо о тесте
                TestId = userTest.TestId,
                TestName = userTest.Test?.Name,
                TestCountOfQuestions = userTest.Test?.CountOfQuestions ?? 0,
                TopicName = userTest.Test?.Topic?.Name,

                Questions = new List<QuestionHistoryDto>()
            };

            // 4) Перебираем UserTestQuestions => QuestionHistoryDto
            foreach (var utq in userTest.UserTestQuestions)
            {
                var qDto = new QuestionHistoryDto
                {
                    UserTestQuestionId = utq.Id,
                    QuestionId = utq.QuestionId,
                    QuestionText = utq.Question.Text,
                    Answers = new List<AnswerHistoryDto>()
                };

                var chosenIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToHashSet();

                // Перебираем все варианты
                foreach (var ans in utq.Question.AnswerOptions)
                {
                    qDto.Answers.Add(new AnswerHistoryDto
                    {
                        AnswerOptionId = ans.Id,
                        Text = ans.Text,
                        IsCorrect = ans.IsCorrect,
                        IsChosen = chosenIds.Contains(ans.Id)
                    });
                }

                dto.Questions.Add(qDto);
            }

            return dto;
        }

        // UserTestService.cs
        public async Task<List<UserTestHistoryDto>> GetAllFullAsync()
        {
            var list = await _userTestRepository.GetAllUserTestFullAsync();

            // Преобразуем каждый userTest в расширенный DTO (см. метод ниже)
            var result = new List<UserTestHistoryDto>();
            foreach (var userTest in list)
            {
                var dto = BuildUserTestHistoryDto(userTest);
                result.Add(dto);
            }
            return result;
        }

        public async Task<List<UserTestHistoryDto>> GetAllByUserEmailFullAsync(string email)
        {
            var list = await _userTestRepository.GetAllByUserEmailFullAsync(email);

            var result = new List<UserTestHistoryDto>();
            foreach (var userTest in list)
            {
                var dto = BuildUserTestHistoryDto(userTest);
                result.Add(dto);
            }
            return result;
        }

        // Предположим, у нас уже есть метод, который формирует UserTestHistoryDto.
        // Можно вынести в отдельный приватный метод:
        private UserTestHistoryDto BuildUserTestHistoryDto(UserTest userTest)
        {
            bool isSurveyTopic = userTest.Test?.Topic?.IsSurveyTopic == true;
            int correctCount = 0;

            if (!isSurveyTopic)
            {
                // Обычная логика подсчёта
                foreach (var utq in userTest.UserTestQuestions)
                {
                    var chosenIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();
                    var correctIds = utq.Question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    bool isCorrect = !correctIds.Except(chosenIds).Any()
                                     && !chosenIds.Except(correctIds).Any();
                    if (isCorrect) correctCount++;
                }
            }

            var dto = new UserTestHistoryDto
            {
                UserTestId = userTest.Id,
                DateCreated = userTest.DateCreated,
                IsPassed = userTest.IsPassed,
                CorrectAnswers = isSurveyTopic ? 0 : correctCount,
                TotalQuestions = userTest.TotalQuestions,

                // User info
                UserId = userTest.UserId,
                UserEmail = userTest.User?.Email,
                UserFullName = userTest.User?.FullName,

                // Test info
                TestId = userTest.TestId,
                TestName = userTest.Test?.Name,
                TestCountOfQuestions = userTest.Test?.CountOfQuestions ?? 0,
                TopicName = userTest.Test?.Topic?.Name,

                // Новое поле — отправим на фронт
                TopicIsSurvey = isSurveyTopic,

                Questions = new List<QuestionHistoryDto>()
            };

            // Заполняем вопросы
            foreach (var utq in userTest.UserTestQuestions)
            {
                var qDto = new QuestionHistoryDto
                {
                    UserTestQuestionId = utq.Id,
                    QuestionId = utq.QuestionId,
                    QuestionText = utq.Question.Text,
                    Answers = new List<AnswerHistoryDto>()
                };

                var chosenIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToHashSet();

                foreach (var ans in utq.Question.AnswerOptions)
                {
                    // Если isSurveyTopic => принудительно ставим isCorrect=true
                    bool finalIsCorrect = isSurveyTopic ? true : ans.IsCorrect;

                    qDto.Answers.Add(new AnswerHistoryDto
                    {
                        AnswerOptionId = ans.Id,
                        Text = ans.Text,
                        IsCorrect = finalIsCorrect,
                        IsChosen = chosenIds.Contains(ans.Id)
                    });
                }
                dto.Questions.Add(qDto);
            }

            return dto;
        }



        public async Task<PaginatedResponse<UserTestHistoryDto>> GetAllFullPaginatedAsync(int page, int pageSize)
        {
            // 1) Берем полный список из _ctx (вместо GetAllUserTestFullAsync())
            var query = _context.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var skip = (page - 1) * pageSize;
            var subset = await query
                .OrderBy(ut => ut.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Собираем UserTestHistoryDto для каждого:
            var items = new List<UserTestHistoryDto>();
            foreach (var userTest in subset)
            {
                var dto = BuildUserTestHistoryDto(userTest);
                items.Add(dto);
            }

            return new PaginatedResponse<UserTestHistoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<PaginatedResponse<UserTestHistoryDto>> GetAllByUserEmailPaginatedAsync(string email, int page, int pageSize)
        {
            var query = _context.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.Question).ThenInclude(q => q.AnswerOptions)
                .Where(ut => ut.User.Email == email);

            var totalItems = await query.CountAsync();

            var skip = (page - 1) * pageSize;
            var subset = await query
                .OrderBy(ut => ut.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = new List<UserTestHistoryDto>();
            foreach (var userTest in subset)
            {
                var dto = BuildUserTestHistoryDto(userTest);
                items.Add(dto);
            }

            return new PaginatedResponse<UserTestHistoryDto>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }


    }
}
