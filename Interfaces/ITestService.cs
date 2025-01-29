using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Логика для управления шаблонами Test.
    /// </summary>
    public interface ITestService
    {
        // Создать шаблон
        Task<TestDto> CreateTemplateAsync(string name, int countOfQuestions, int? topicId);

        // Получить шаблон
        Task<TestDto?> GetTestByIdAsync(int id);
        Task<IEnumerable<TestDto>> GetAllTestsAsync();

        // Обновить шаблон (например, изменить количество вопросов, тему и т.д.)
        Task<TestDto> UpdateTestAsync(int id, string newName, int countOfQuestions, int? topicId);

        // Удалить
        Task DeleteTestAsync(int id);
    }
}
