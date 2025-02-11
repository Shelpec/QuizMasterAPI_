using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITestQuestionRepository : IGenericRepository<TestQuestion>
    {
        Task<IEnumerable<TestQuestion>> GetAllByTestIdAsync(int testId);
    }
}
