using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Логика для управления шаблонами Test.
    /// </summary>
    public interface ITestService
    {
        Task<TestDto> CreateTemplateAsync(string name, int countOfQuestions, int topicId, bool isPrivate);
        Task<TestDto> UpdateTestAsync(int id, string newName, int countOfQuestions, int? topicId, bool isPrivate);

        // Получить шаблон
        Task<TestDto?> GetTestByIdAsync(int id);
        Task<IEnumerable<TestDto>> GetAllTestsAsync();


        // Удалить
        Task DeleteTestAsync(int id);

        Task<PaginatedResponse<TestDto>> GetAllTestsPaginatedAsync(int page, int pageSize, string? currentUserId, bool isAdmin);


    }
}
