using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestAccessRepository : IGenericRepository<TestAccess>
    {
        /// <summary>
        /// Проверить, есть ли у указанного пользователя доступ к тесту
        /// </summary>
        Task<bool> HasAccessAsync(int testId, string userId);

        /// <summary>
        /// Получить список пользователей (UserId) у кого есть доступ к тесту
        /// </summary>
        Task<List<TestAccess>> GetTestAccessListAsync(int testId);
    }
}
