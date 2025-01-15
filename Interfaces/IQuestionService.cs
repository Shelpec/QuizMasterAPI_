using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IQuestionService
    {
        Task<IEnumerable<Question>> GetAllQuestions(); // Получение всех вопросов
        Task<Question> GetQuestion(int id); // Получение вопроса по ID
        Task<Question> CreateQuestion(CreateQuestionDto questionDto); // Создание вопроса
        Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto); // Обновление вопроса
        Task<bool> DeleteQuestion(int id); // Удаление вопроса
        Task<bool> CheckAnswer(int questionId, int selectedAnswerId); // Проверка ответа
    }
}
