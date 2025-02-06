using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestAccessService
    {
        Task AddUserAccessAsync(int testId, string userId);
        Task RemoveUserAccessAsync(int testId, string userId);
        Task<List<string>> GetAllUsersForTestAsync(int testId);
        Task<bool> HasAccessAsync(int testId, string userId);
    }

}
