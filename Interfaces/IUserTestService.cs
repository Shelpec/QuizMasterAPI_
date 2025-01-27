using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Логика прохождения теста конкретным пользователем.
    /// </summary>
    public interface IUserTestService
    {
        /// <summary>
        /// Пользователь (userId) начинает прохождение теста (testId).
        /// Генерируются случайные вопросы по настройкам шаблона.
        /// </summary>
        Task<UserTest> StartTestAsync(int testId, string userId);

        /// <summary>
        /// Проверяем ответы пользователя по UserTest (id).
        /// Возвращаем результат с количеством правильных и т.д.
        /// </summary>
        Task<TestCheckResultDto> CheckUserTestAnswersAsync(int userTestId, List<TestAnswerValidationDto> answers, string userId);
    }
}
