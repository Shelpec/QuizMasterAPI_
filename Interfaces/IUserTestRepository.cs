using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestRepository : IGenericRepository<UserTest>
    {
        Task<UserTest?> GetUserTestWithQuestionsAsync(int userTestId);
        Task<UserTest?> GetUserTestAsync(int userTestId);
        // Новый метод:
        Task<UserTest?> GetUserTestFullAsync(int userTestId);
        Task<List<UserTest>> GetAllUserTestFullAsync();
        Task<List<UserTest>> GetAllByUserEmailFullAsync(string email);
        Task<UserTest?> GetUserTestWithEverythingAsync(int userTestId);


    }
}
