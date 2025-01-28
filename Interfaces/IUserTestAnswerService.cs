using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestAnswerService
    {
        // Сохранить ответы пользователя
        Task SaveAnswersAsync(int userTestId, List<UserAnswerSubmitDto> answers, string userId);

        // Проверить ответы (и вернуть результат)
        Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId);
    }
}
