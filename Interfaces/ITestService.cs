using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Логика для управления шаблонами Test.
    /// </summary>
    public interface ITestService
    {
        // Создать шаблон
        Task<Test> CreateTemplateAsync(string name, int countOfQuestions, int? topicId);

        // Получить шаблон
        Task<Test?> GetTestByIdAsync(int id);
        Task<IEnumerable<Test>> GetAllTestsAsync();

        // Обновить шаблон (например, изменить количество вопросов, тему и т.д.)
        Task<Test> UpdateTestAsync(int id, string newName, int countOfQuestions, int? topicId);

        // Удалить
        Task DeleteTestAsync(int id);
    }
}
