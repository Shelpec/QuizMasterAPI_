using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly ILogger<TestService> _logger;
        private readonly IMapper _mapper;
        private readonly QuizDbContext _ctx; // для LINQ-запроса

        public TestService(
            ITestRepository testRepository,
            ITopicRepository topicRepository,
            ILogger<TestService> logger,
            IMapper mapper,
            QuizDbContext ctx)
        {
            _testRepository = testRepository;
            _topicRepository = topicRepository;
            _logger = logger;
            _mapper = mapper;
            _ctx = ctx;
        }

        public async Task<TestDto> CreateTemplateAsync(
            string name,
            int countOfQuestions,
            int topicId,
            bool isPrivate
        )
        {
            var topic = await _topicRepository.GetTopicByIdAsync(topicId);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={topicId} not found");



            _logger.LogInformation("CreateTemplateAsync(Name={Name}, isPrivate={Priv})", name, isPrivate);
            var test = new Test
            {
                Name = name,
                TopicId = topicId,
                Topic = topic,
                CountOfQuestions = countOfQuestions,
                CreatedAt = DateTime.UtcNow,
                IsPrivate = isPrivate,
                IsSurvey = topic.IsSurveyTopic

            };

            await _testRepository.AddAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }



        public async Task<TestDto?> GetTestByIdAsync(int id)
        {
            _logger.LogInformation("GetTestByIdAsync(Id={Id})", id);
            try
            {
                var test = await _testRepository.GetTestByIdAsync(id);
                if (test == null)
                    return null;

                return _mapper.Map<TestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetTestByIdAsync(Id={Id})", id);
                throw;
            }
        }

        public async Task<IEnumerable<TestDto>> GetAllTestsAsync()
        {
            _logger.LogInformation("GetAllTestsAsync()");
            try
            {
                var tests = await _testRepository.GetAllTestsAsync();
                // tests уже со связанным Topic
                return _mapper.Map<IEnumerable<TestDto>>(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllTestsAsync()");
                throw;
            }
        }

        public async Task<TestDto> UpdateTestAsync(
            int id,
            string newName,
            int countOfQuestions,
            int? topicId,
            bool isPrivate
        )
        {
            _logger.LogInformation("UpdateTestAsync(Id={Id}, isPrivate={Priv}, isSurvey={Surv})", id, isPrivate);
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={id} not found.");

            test.Name = newName;
            test.CountOfQuestions = countOfQuestions;
            test.TopicId = topicId;
            test.IsPrivate = isPrivate;

            await _testRepository.UpdateAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }



        public async Task DeleteTestAsync(int id)
        {
            _logger.LogInformation("DeleteTestAsync(Id={Id})", id);
            try
            {
                var test = await _testRepository.GetTestByIdAsync(id);
                if (test == null)
                    throw new KeyNotFoundException($"Шаблон теста с ID={id} не найден.");

                await _testRepository.DeleteAsync(test);
                await _testRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в DeleteTestAsync(Id={Id})", id);
                throw;
            }
        }

        public async Task<PaginatedResponse<TestDto>> GetAllTestsPaginatedAsync(
            int page,
            int pageSize,
            string? currentUserId,
            bool isAdmin)
        {
            _logger.LogInformation("GetAllTestsPaginatedAsync(page={Page}, pageSize={Size}, userId={User}, isAdmin={Admin})", page, pageSize, currentUserId, isAdmin);

            // Базовый запрос
            var query = _ctx.Tests
                .Include(t => t.Topic)
                .AsQueryable();

            if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
            {
                // Фильтруем:
                //  1) берем публичные  (IsPrivate == false)
                //  2) ИЛИ те, где в TestAccess есть currentUserId
                // Нам нужен Join с TestAccess.
                // Решение: делаем left join, но проще через subquery
                var testIdsWithAccess = _ctx.TestAccesses
                    .Where(ta => ta.UserId == currentUserId)
                    .Select(ta => ta.TestId)
                    .Distinct();

                query = query.Where(t => t.IsPrivate == false || testIdsWithAccess.Contains(t.Id));
            }
            // Если user is Admin => ничего не фильтруем.

            var totalItems = await query.CountAsync();

            var skip = (page - 1) * pageSize;
            var tests = await query
                .OrderBy(t => t.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var dtos = tests.Select(t => _mapper.Map<TestDto>(t)).ToList();

            return new PaginatedResponse<TestDto>
            {
                Items = dtos,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }


    }
}
