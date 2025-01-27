using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestRepository : IGenericRepository<Test>
    {
        Task<Test?> GetTestByIdAsync(int id);
        Task<IEnumerable<Test>> GetAllTestsAsync();
    }
}
