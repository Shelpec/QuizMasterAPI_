using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllQuestions();
        Task<IEnumerable<QuestionDto>> GetAllQuestionsDto();
        Task<Question> GetQuestion(int id);
        Task<QuestionDto> GetQuestionDto(int id);
        Task<Question> CreateQuestion(CreateQuestionDto questionDto);
        Task<Question> UpdateQuestion(int id, UpdateQuestionDto dto);
        Task<bool> DeleteQuestion(int id);
        Task<bool> CheckAnswer(int questionId, int selectedAnswerId);
        Task<IEnumerable<QuestionDto>> GetRandomQuestions(int count);
        Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers);
        Task<PaginatedResponse<QuestionDto>> GetAllQuestionsPaginatedAsync(int page, int pageSize);
    }
}
