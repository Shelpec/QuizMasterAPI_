using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TestAccessService : ITestAccessService
    {
        private readonly ITestAccessRepository _repo;
        private readonly ILogger<TestAccessService> _logger;

        public TestAccessService(ITestAccessRepository repo, ILogger<TestAccessService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task AddUserAccessAsync(int testId, string userId)
        {
            _logger.LogInformation("AddUserAccessAsync(TestId={TestId}, UserId={UserId})", testId, userId);
            // Проверим, нет ли уже
            if (await _repo.HasAccessAsync(testId, userId))
            {
                _logger.LogInformation("Пользователь {UserId} уже имеет доступ к TestId={TestId}", userId, testId);
                return; // или кинуть исключение, если нужно
            }
            var entity = new TestAccess
            {
                TestId = testId,
                UserId = userId
            };
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
        }

        public async Task RemoveUserAccessAsync(int testId, string userId)
        {
            _logger.LogInformation("RemoveUserAccessAsync(TestId={TestId}, UserId={UserId})", testId, userId);
            var list = await _repo.FindAsync(ta => ta.TestId == testId && ta.UserId == userId);
            var access = list.FirstOrDefault();
            if (access == null)
            {
                _logger.LogWarning("Не найден TestAccess для testId={TestId}, userId={UserId}", testId, userId);
                return; // или кинуть исключение
            }
            await _repo.DeleteAsync(access);
            await _repo.SaveChangesAsync();
        }

        public async Task<List<string>> GetAllUsersForTestAsync(int testId)
        {
            var list = await _repo.GetTestAccessListAsync(testId);
            return list.Select(ta => ta.UserId).ToList();
        }

        public async Task<bool> HasAccessAsync(int testId, string userId)
        {
            return await _repo.HasAccessAsync(testId, userId);
        }
    }
}
