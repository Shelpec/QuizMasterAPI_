using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;
using QuizMasterAPI.Repositories;

using System.Linq;

namespace QuizMasterAPI.Services
{
    public class UserTestService : IUserTestService
    {
        private readonly IUserTestRepository _userTestRepository;
        private readonly ITestRepository _testRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITestQuestionRepository _testQuestionRepository;
        private readonly QuizDbContext _context;
        private readonly ILogger<UserTestService> _logger;
        private readonly IMapper _mapper; // <-- AutoMapper

        public UserTestService(
            IUserTestRepository userTestRepository,
            ITestRepository testRepository,
            IQuestionRepository questionRepository,
            ITestQuestionRepository testQuestionRepository,
            QuizDbContext context,
            ILogger<UserTestService> logger,
            IMapper mapper) // <-- получаем из DI
        {
            _userTestRepository = userTestRepository;
            _testRepository = testRepository;
            _questionRepository = questionRepository;
            _testQuestionRepository = testQuestionRepository;
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
            _logger.LogInformation("Старт теста {TestId} для пользователя {UserId}", testId, userId);

            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Тест ID={testId} не найден.");

            // Собираем вопросы
            List<Question> questions = test.IsRandom
                ? await _questionRepository.GetRandomQuestionsAsync(test.CountOfQuestions, test.TopicId)
                : test.TestQuestions.Select(tq => tq.Question).ToList();

            if (!questions.Any())
                throw new Exception($"В тесте ID={testId} нет доступных вопросов.");

            // Создаём UserTest
            var userTest = new UserTest
            {
                UserId = userId,
                TestId = test.Id,
                TotalQuestions = questions.Count,
                CorrectAnswers = 0,
                IsPassed = false,
                DateCreated = DateTime.UtcNow,

                // Новые поля для времени
                StartTime = DateTime.UtcNow
            };

            // Если у теста есть лимит (TimeLimitMinutes)
            if (test.TimeLimitMinutes.HasValue && test.TimeLimitMinutes.Value > 0)
            {
                // expireTime
                userTest.ExpireTime = userTest.StartTime.Value.AddMinutes(test.TimeLimitMinutes.Value);
            }

            await _userTestRepository.AddAsync(userTest);
            await _userTestRepository.SaveChangesAsync();

            // Добавляем UserTestQuestions
            var userTestQuestions = questions.Select(q => new UserTestQuestion
            {
                UserTestId = userTest.Id,
                QuestionId = q.Id
            }).ToList();

            _context.UserTestQuestions.AddRange(userTestQuestions);
            await _context.SaveChangesAsync();

            var createdUserTest = await _userTestRepository.GetUserTestWithQuestionsAsync(userTest.Id);
            return _mapper.Map<UserTestDto>(createdUserTest);
        }

        // ================================
        // Получение истории по UserTest
        // ================================
        public async Task<UserTestHistoryDto?> GetFullUserTestAsync(int userTestId)
        {
            var userTest = await _userTestRepository.GetUserTestFullAsync(userTestId);
            if (userTest == null)
                return null;

            // Подсчитаем кол-во правильных ответов
            int correctCount = 0;
            var totalQuestions = userTest.UserTestQuestions.Count;

            // Формируем результат
            var dto = new UserTestHistoryDto
            {
                UserTestId = userTest.Id,
                DateCreated = userTest.DateCreated,
                IsPassed = userTest.IsPassed,
                CorrectAnswers = 0, // пока 0, ниже вычислим
                TotalQuestions = totalQuestions,

                UserId = userTest.UserId,
                UserEmail = userTest.User?.Email,
                UserFullName = userTest.User?.FullName,

                TestId = userTest.TestId,
                TestName = userTest.Test?.Name,
                TestCountOfQuestions = userTest.Test?.CountOfQuestions ?? 0,
                TopicName = userTest.Test?.Topic?.Name,

                Questions = new List<QuestionHistoryDto>()
            };

            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = utq.Question;
                if (question == null) continue;

                // Собираем IDs выбранных (не-null) AnswerOption
                var chosenIds = utq.UserTestAnswers
                    .Where(a => a.AnswerOptionId.HasValue)
                    .Select(a => a.AnswerOptionId!.Value) // ! - убираем nullable
                    .ToList();

                // Собираем IDs правильных
                var correctIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                bool isCorrect = false;

                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    // Если не надо проверять, просто false/true. Ниже оставим false, напр.
                    // Или если хотим проверять, сравнить userTestAnswers.FirstOrDefault()?.UserTextAnswer ...
                    isCorrect = false;
                }
                else if (question.QuestionType == QuestionTypeEnum.Survey)
                {
                    // Все ответы правильные
                    isCorrect = true;
                }
                else
                {
                    isCorrect = !correctIds.Except(chosenIds).Any()
                                && !chosenIds.Except(correctIds).Any();
                }

                if (isCorrect) correctCount++;

                // Заполняем QuestionHistoryDto
                var qDto = new QuestionHistoryDto
                {
                    UserTestQuestionId = utq.Id,
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    QuestionType = question.QuestionType,
                    AnswerOptions = new List<AnswerHistoryDto>()
                };

                // Перебираем все варианты
                var chosenSet = chosenIds.ToHashSet();
                foreach (var ans in question.AnswerOptions)
                {
                    qDto.AnswerOptions.Add(new AnswerHistoryDto
                    {
                        AnswerOptionId = ans.Id,
                        Text = ans.Text,
                        IsCorrect = ans.IsCorrect,
                        IsChosen = chosenSet.Contains(ans.Id)
                    });
                }

                // Если вопрос текстовый (OpenText) => добавим UserTextAnswer
                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    var userText = utq.UserTestAnswers.FirstOrDefault()?.UserTextAnswer;
                    qDto.AnswerOptions.Add(new AnswerHistoryDto
                    {
                        UserTextAnswer = userText
                    });
                }

                dto.Questions.Add(qDto);
            }

            dto.CorrectAnswers = correctCount;
            return dto;
        }

        public async Task<List<UserTestHistoryDto>> GetAllFullAsync()
        {
            var list = await _userTestRepository.GetAllUserTestFullAsync();
            var result = new List<UserTestHistoryDto>();
            foreach (var userTest in list)
            {
                var item = BuildUserTestHistoryDto(userTest);
                result.Add(item);
            }
            return result;
        }

        public async Task<List<UserTestHistoryDto>> GetAllByUserEmailFullAsync(string email)
        {
            var list = await _userTestRepository.GetAllByUserEmailFullAsync(email);
            var result = new List<UserTestHistoryDto>();
            foreach (var userTest in list)
            {
                var item = BuildUserTestHistoryDto(userTest);
                result.Add(item);
            }
            return result;
        }

        private UserTestHistoryDto BuildUserTestHistoryDto(UserTest userTest)
        {
            int correctCount = 0;
            var totalQuestions = userTest.UserTestQuestions.Count;

            var dto = new UserTestHistoryDto
            {
                UserTestId = userTest.Id,
                DateCreated = userTest.DateCreated,
                IsPassed = userTest.IsPassed,
                CorrectAnswers = 0,
                TotalQuestions = totalQuestions,

                UserId = userTest.UserId,
                UserEmail = userTest.User?.Email,
                UserFullName = userTest.User?.FullName,

                TestId = userTest.TestId,
                TestName = userTest.Test?.Name,
                TestCountOfQuestions = userTest.Test?.CountOfQuestions ?? 0,
                TopicName = userTest.Test?.Topic?.Name,

                StartTime = userTest.StartTime,
                EndTime = userTest.EndTime,
                TimeSpentSeconds = userTest.TimeSpentSeconds,

                Questions = new List<QuestionHistoryDto>()
            };

            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = utq.Question;
                if (question == null) continue;

                // Собираем ID выбранных (Single/Multiple)
                var chosenIds = utq.UserTestAnswers
                    .Where(a => a.AnswerOptionId.HasValue)
                    .Select(a => a.AnswerOptionId!.Value)
                    .ToList();

                // Собираем ID «правильных»
                var correctIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                // Определяем isCorrect
                bool isCorrect = false;

                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    // Всегда считаем правильным
                    isCorrect = true;
                }
                else if (question.QuestionType == QuestionTypeEnum.Survey)
                {
                    // Тоже считаем правильным
                    isCorrect = true;
                }
                else
                {
                    // SingleChoice / MultipleChoice
                    isCorrect = !correctIds.Except(chosenIds).Any()
                                && !chosenIds.Except(correctIds).Any();
                }

                if (isCorrect)
                    correctCount++;

                // Формируем DTO для одного вопроса
                var qDto = new QuestionHistoryDto
                {
                    UserTestQuestionId = utq.Id,
                    QuestionId = question.Id,
                    QuestionText = question.Text,
                    QuestionType = question.QuestionType,
                    AnswerOptions = new List<AnswerHistoryDto>()
                };

                // 1) Если вопрос OpenText => добавляем один объект AnswerHistoryDto
                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    var userText = utq.UserTestAnswers.FirstOrDefault()?.UserTextAnswer;
                    qDto.AnswerOptions.Add(new AnswerHistoryDto
                    {
                        AnswerOptionId = null,
                        Text = null,
                        // Ставим IsCorrect = true => зелёный
                        IsCorrect = true,
                        // Говорим что «выбран» (для визуализации)
                        IsChosen = true,
                        UserTextAnswer = userText
                    });
                }
                // 2) Если вопрос Survey => все варианты показываем «правильными»
                else if (question.QuestionType == QuestionTypeEnum.Survey)
                {
                    var chosenSet = chosenIds.ToHashSet();
                    foreach (var ansOpt in question.AnswerOptions)
                    {
                        qDto.AnswerOptions.Add(new AnswerHistoryDto
                        {
                            AnswerOptionId = ansOpt.Id,
                            Text = ansOpt.Text,
                            // Всегда правильный => зелёный
                            IsCorrect = true,
                            // Выбран ли пользователь
                            IsChosen = chosenSet.Contains(ansOpt.Id),
                            UserTextAnswer = null
                        });
                    }
                }
                // 3) SingleChoice/MultipleChoice — обычная логика
                else
                {
                    var chosenSet = chosenIds.ToHashSet();
                    foreach (var ansOpt in question.AnswerOptions)
                    {
                        qDto.AnswerOptions.Add(new AnswerHistoryDto
                        {
                            AnswerOptionId = ansOpt.Id,
                            Text = ansOpt.Text,
                            IsCorrect = ansOpt.IsCorrect,
                            IsChosen = chosenSet.Contains(ansOpt.Id),
                            UserTextAnswer = null
                        });
                    }
                }

                dto.Questions.Add(qDto);
            }

            dto.CorrectAnswers = correctCount;
            return dto;
        }




        public async Task<PaginatedResponse<UserTestHistoryDto>> GetAllFullPaginatedAsync(int page, int pageSize)
        {
            var query = _context.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.Question).ThenInclude(q => q.AnswerOptions)
                .AsQueryable();

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
                var item = BuildUserTestHistoryDto(userTest);
                items.Add(item);
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
                var item = BuildUserTestHistoryDto(userTest);
                items.Add(item);
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

        public async Task<List<UserTestHistoryDto>> GetAllHistoryByTestId(int testId)
        {
            // Грузим все UserTest с нужным TestId
            var userTests = await _context.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions).ThenInclude(utq => utq.Question).ThenInclude(q => q.AnswerOptions)
                .Where(ut => ut.TestId == testId)
                .ToListAsync();

            // Собираем список DTO
            var result = new List<UserTestHistoryDto>();
            foreach (var userTest in userTests)
            {
                var dto = BuildUserTestHistoryDto(userTest);
                result.Add(dto);
            }
            return result;
        }
    }
}
