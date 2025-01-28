using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestAnswerRepository : IGenericRepository<UserTestAnswer>
    {
        // Допустим, хотим метод получения всех ответов для конкретного UserTestQuestion
        Task<List<UserTestAnswer>> GetAnswersByUserTestQuestionId(int userTestQuestionId);
    }
}
