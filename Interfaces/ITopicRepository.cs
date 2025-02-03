// Interfaces/ITopicRepository.cs
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface ITopicRepository : IGenericRepository<Topic>
    {
        Task<IEnumerable<Topic>> GetAllTopicsAsync();
        Task<Topic?> GetTopicByIdAsync(int id);
    }
}
