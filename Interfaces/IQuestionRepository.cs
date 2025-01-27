using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        Task<List<Question>> GetQuestionsWithAnswersByIdsAsync(List<int> questionIds);
        Task<List<Question>> GetRandomQuestionsAsync(int count);
        Task<List<Question>> GetRandomQuestionsAsync(int count, int? topicId);

        Task<Question?> GetQuestionByIdAsync(int id);

    }
}
