using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllQuestions();
        Task<IEnumerable<QuestionDto>> GetAllQuestionsDto();
        Task<Question> GetQuestion(int id);
        Task<Question> CreateQuestion(CreateQuestionDto questionDto);
        Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto);
        Task<bool> DeleteQuestion(int id);
        Task<bool> CheckAnswer(int questionId, int selectedAnswerId);

        Task<QuestionDto> GetQuestionDto(int id);
        Task<IEnumerable<QuestionDto>> GetRandomQuestions(int count);
        Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers);
    }
}
