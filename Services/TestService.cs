using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;

        public TestService(ITestRepository testRepository)
        {
            _testRepository = testRepository;
        }

        public async Task<Test> CreateTemplateAsync(string name, int countOfQuestions, int? topicId)
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

            return test;
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _testRepository.GetTestByIdAsync(id);
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _testRepository.GetAllTestsAsync();
        }

        public async Task<Test> UpdateTestAsync(int id, string newName, int countOfQuestions, int? topicId)
        {
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Шаблон теста с ID={id} не найден.");

            test.Name = newName;
            test.CountOfQuestions = countOfQuestions;
            test.TopicId = topicId;

            await _testRepository.UpdateAsync(test);
            await _testRepository.SaveChangesAsync();

            return test;
        }

        public async Task DeleteTestAsync(int id)
        {
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Шаблон теста с ID={id} не найден.");

            await _testRepository.DeleteAsync(test);
            await _testRepository.SaveChangesAsync();
        }


    }
}
