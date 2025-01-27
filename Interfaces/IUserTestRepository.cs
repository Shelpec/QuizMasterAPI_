using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestRepository : IGenericRepository<UserTest>
    {
        Task<UserTest?> GetUserTestWithQuestionsAsync(int userTestId);
        Task<UserTest?> GetUserTestAsync(int userTestId);
    }
}
