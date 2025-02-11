using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;  // <-- для TestTypeEnum

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<TestService> _logger;
        private readonly IMapper _mapper;
        private readonly QuizDbContext _ctx; // для LINQ-запросов, пагинации

        private readonly ITestQuestionRepository _testQuestionRepository;


        public TestService(
            ITestRepository testRepository,
            ITopicRepository topicRepository,
            IQuestionRepository questionRepository,
            ITestQuestionRepository testQuestionRepository,
            ILogger<TestService> logger,
            IMapper mapper,
            QuizDbContext ctx)
        {
            _testRepository = testRepository;
            _topicRepository = topicRepository;
            _questionRepository = questionRepository;
            _testQuestionRepository = testQuestionRepository;
            _logger = logger;
            _mapper = mapper;
            _ctx = ctx;
        }

        /// <summary>
        /// Создание нового теста.
        /// Добавляем isRandom, testType (enum).
        /// </summary>
        public async Task<TestDto> CreateTemplateAsync(
            string name,
            int countOfQuestions,
            int topicId,
            bool isPrivate,
            bool isRandom,
            string? testType
        )
        {
            var topic = await _topicRepository.GetTopicByIdAsync(topicId);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={topicId} not found");

            // Если testType = null или пусто -> используем QuestionsOnly
            // Иначе пробуем конвертировать строку в TestTypeEnum
            var finalTestType = TestTypeEnum.QuestionsOnly;
            if (!string.IsNullOrEmpty(testType))
            {
                if (!Enum.TryParse<TestTypeEnum>(testType, ignoreCase: true, out finalTestType))
                {
                    throw new ArgumentException($"Invalid testType value: {testType}. Expected: QuestionsOnly, SurveyOnly or Mixed.");
                }
            }

            _logger.LogInformation("CreateTemplateAsync(Name={Name}, isPrivate={Priv}, isRandom={Rand}, testType={Type})",
                name, isPrivate, isRandom, finalTestType);

            var test = new Test
            {
                Name = name,
                TopicId = topicId,
                Topic = topic,
                CountOfQuestions = countOfQuestions,
                CreatedAt = DateTime.UtcNow,
                IsPrivate = isPrivate,
                IsRandom = isRandom,
                TestType = finalTestType
            };

            await _testRepository.AddAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }

        public async Task<TestDto?> GetTestByIdAsync(int id)
        {
            _logger.LogInformation("GetTestByIdAsync(Id={Id})", id);
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                return null;

            return _mapper.Map<TestDto>(test);
        }


        public async Task<IEnumerable<TestDto>> GetAllTestsAsync()
        {
            _logger.LogInformation("GetAllTestsAsync()");
            var tests = await _testRepository.GetAllTestsAsync();
            return _mapper.Map<IEnumerable<TestDto>>(tests);
        }


        /// <summary>
        /// Обновление теста: учитываем isRandom, testType (enum).
        /// </summary>
        public async Task<TestDto> UpdateTestAsync(
            int id,
            string newName,
            int countOfQuestions,
            int? topicId,
            bool isPrivate,
            bool isRandom,
            string? testType
        )
        {
            _logger.LogInformation("UpdateTestAsync(Id={Id}, isPrivate={Priv}, isRandom={Rand}, testType={Type})",
                id, isPrivate, isRandom, testType);

            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={id} not found.");

            test.Name = newName;
            test.CountOfQuestions = countOfQuestions;
            test.IsPrivate = isPrivate;
            test.IsRandom = isRandom;

            // Если testType не пусто -> пытаемся распарсить
            if (!string.IsNullOrEmpty(testType))
            {
                if (!Enum.TryParse<TestTypeEnum>(testType, ignoreCase: true, out var parsedType))
                {
                    throw new ArgumentException($"Invalid testType value: {testType}. Expected: QuestionsOnly, SurveyOnly or Mixed.");
                }
                test.TestType = parsedType;
            }

            // Если хотим менять TopicId
            if (topicId.HasValue)
            {
                var topic = await _topicRepository.GetTopicByIdAsync(topicId.Value);
                if (topic == null)
                    throw new KeyNotFoundException($"Topic with ID={topicId.Value} not found.");

                test.TopicId = topicId.Value;
                test.Topic = topic;
            }

            await _testRepository.UpdateAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }

        public async Task DeleteTestAsync(int id)
        {
            _logger.LogInformation("DeleteTestAsync(Id={Id})", id);
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Шаблон теста с ID={id} не найден.");

            await _testRepository.DeleteAsync(test);
            await _testRepository.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<TestDto>> GetAllTestsPaginatedAsync(
            int page,
            int pageSize,
            string? currentUserId,
            bool isAdmin)
        {
            _logger.LogInformation("GetAllTestsPaginatedAsync(page={Page}, pageSize={Size}, userId={User}, isAdmin={Admin})",
                page, pageSize, currentUserId, isAdmin);

            var query = _ctx.Tests
                .Include(t => t.Topic)
                .AsQueryable();

            // Если пользователь не админ, показываем либо публичные,
            // либо приватные, на которые у него есть доступ
            if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
            {
                var testIdsWithAccess = _ctx.TestAccesses
                    .Where(ta => ta.UserId == currentUserId)
                    .Select(ta => ta.TestId)
                    .Distinct();

                query = query.Where(t => t.IsPrivate == false || testIdsWithAccess.Contains(t.Id));
            }

            var totalItems = await query.CountAsync();

            var skip = (page - 1) * pageSize;
            var tests = await query
                .OrderBy(t => t.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var dtos = _mapper.Map<List<TestDto>>(tests);

            return new PaginatedResponse<TestDto>
            {
                Items = dtos,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<TestDto> AddQuestionToTest(int testId, int questionId)
        {
            _logger.LogInformation("Добавляем вопрос {QuestionId} в тест {TestId}", questionId, testId);

            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={testId} not found.");

            var question = await _ctx.Questions.FindAsync(questionId);
            if (question == null)
                throw new KeyNotFoundException($"Question with ID={questionId} not found.");

            var testQuestion = new TestQuestion
            {
                TestId = testId,
                QuestionId = questionId
            };

            await _testQuestionRepository.AddAsync(testQuestion);
            await _testQuestionRepository.SaveChangesAsync();

            var updatedTest = await _testRepository.GetTestByIdAsync(testId);
            return _mapper.Map<TestDto>(updatedTest);
        }

        public async Task<TestDto> RemoveQuestionFromTest(int testId, int questionId)
        {
            _logger.LogInformation("Удаляем вопрос {QuestionId} из теста {TestId}", questionId, testId);

            var testQuestion = await _ctx.TestQuestions
                .FirstOrDefaultAsync(tq => tq.TestId == testId && tq.QuestionId == questionId);

            if (testQuestion == null)
                throw new KeyNotFoundException($"Question ID={questionId} not found in Test ID={testId}.");

            await _testQuestionRepository.DeleteAsync(testQuestion);
            await _testQuestionRepository.SaveChangesAsync();

            var updatedTest = await _testRepository.GetTestByIdAsync(testId);
            return _mapper.Map<TestDto>(updatedTest);
        }

        public async Task<List<QuestionDto>> GetQuestionsByTestId(int testId)
        {
            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Тест с ID={testId} не найден.");

            List<Question> questions;

            if (test.IsRandom)
            {
                questions = await _questionRepository.GetRandomQuestionsAsync(test.CountOfQuestions, test.TopicId);
            }
            else
            {
                var testQuestions = await _testQuestionRepository.GetAllByTestIdAsync(testId);
                questions = testQuestions.Select(tq => tq.Question).ToList();
            }

            return _mapper.Map<List<QuestionDto>>(questions);
        }

        public async Task<List<QuestionDto>> GetTestQuestionsAsync(int testId)
        {
            _logger.LogInformation("Получаем вопросы для теста {TestId}", testId);

            var questions = await _testRepository.GetTestQuestionsAsync(testId);
            if (questions == null || questions.Count == 0)
            {
                _logger.LogWarning("Для теста {TestId} нет вопросов", testId);
                return new List<QuestionDto>();
            }

            return _mapper.Map<List<QuestionDto>>(questions);
        }

    }
}
