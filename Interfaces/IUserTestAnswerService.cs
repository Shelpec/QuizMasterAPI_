using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestAnswerService
    {
        Task SaveAnswersAsync(int userTestId, List<UserAnswerSubmitDto> answers, string userId);
        Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId);
    }

}
