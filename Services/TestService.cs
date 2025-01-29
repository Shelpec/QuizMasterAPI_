using AutoMapper;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ILogger<TestService> _logger;
        private readonly IMapper _mapper;

        public TestService(ITestRepository testRepository, ILogger<TestService> logger, IMapper mapper)
        {
            _testRepository = testRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<TestDto> CreateTemplateAsync(string name, int countOfQuestions, int? topicId)
        {
            _logger.LogInformation("CreateTemplateAsync(Name={Name}, count={Count}, topic={Topic})", name, countOfQuestions, topicId);
            try
            {
                var test = new Test
                {
                    Name = name,
                    TopicId = topicId,
                    CountOfQuestions = countOfQuestions,
                    CreatedAt = DateTime.UtcNow
                };

                await _testRepository.AddAsync(test);
                await _testRepository.SaveChangesAsync();

                return _mapper.Map<TestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CreateTemplateAsync(Name={Name})", name);
                throw;
            }
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
                return _mapper.Map<IEnumerable<TestDto>>(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllTestsAsync()");
                throw;
            }
        }

        public async Task<TestDto> UpdateTestAsync(int id, string newName, int countOfQuestions, int? topicId)
        {
            _logger.LogInformation("UpdateTestAsync(Id={Id}, Name={Name}, Count={Count})", id, newName, countOfQuestions);
            try
            {
                var test = await _testRepository.GetTestByIdAsync(id);
                if (test == null)
                    throw new KeyNotFoundException($"Шаблон теста с ID={id} не найден.");

                test.Name = newName;
                test.CountOfQuestions = countOfQuestions;
                test.TopicId = topicId;

                await _testRepository.UpdateAsync(test);
                await _testRepository.SaveChangesAsync();
                return _mapper.Map<TestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в UpdateTestAsync(Id={Id})", id);
                throw;
            }
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
    }
}
