using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestRepository
    {
        Task<Test> CreateTestAsync(Test test);
        Task<Test?> GetTestByIdAsync(int id);
        Task<IEnumerable<Test>> GetAllTestsAsync();
    }
}
