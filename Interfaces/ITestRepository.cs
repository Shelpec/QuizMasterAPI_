namespace QuizMasterAPI.Interfaces
{
    public interface ITestRepository : IGenericRepository<Models.Entities.Test>
    {
        Task<Models.Entities.Test?> GetTestByIdAsync(int id);
        Task<IEnumerable<Models.Entities.Test>> GetAllTestsAsync();
    }
}
