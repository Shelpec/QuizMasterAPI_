using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestService
    {
        /// <summary>
        /// Создает тест со случайным набором вопросов.
        /// </summary>
        /// <param name="questionCount">Количество вопросов</param>
        /// <returns>Созданный тест</returns>
        Task<Test> CreateTestAsync(int questionCount,int? topicId, string userId);

        /// <summary>
        /// Получить тест по его Id
        /// </summary>
        Task<Test?> GetTestByIdAsync(int id);

        /// <summary>
        /// Получить все тесты
        /// </summary>
        Task<IEnumerable<Test>> GetAllTestsAsync();

        Task<TestCheckResultDto> CheckTestAnswersAsync(int testId, List<TestAnswerValidationDto> userAnswers);

        // Новый метод: обновление теста
        Task<Test> UpdateTestAsync(int id, List<int> questionIds);

        // Новый метод: удаление теста
        Task DeleteTestAsync(int id);
    }
}
