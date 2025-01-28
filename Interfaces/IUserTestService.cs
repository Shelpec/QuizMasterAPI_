using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    public interface IUserTestService
    {
        // Создать UserTest (без логики ответов)
        Task<UserTest> CreateAsync(UserTest userTest);

        // Получить UserTest
        Task<UserTest?> GetByIdAsync(int id);

        // Обновить (например, IsPassed, CorrectAnswers, если надо)
        Task UpdateAsync(UserTest userTest);

        // Удалить
        Task DeleteAsync(int id);

        // Дополнительно: "Старт теста" — 
        // метод, генерирующий UserTestQuestions, если хотите 
        Task<UserTestDto> StartTestAsync(int testId, string userId);
    }
}
