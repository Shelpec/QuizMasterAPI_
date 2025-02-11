﻿using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Логика для управления шаблонами Test.
    /// </summary>
    public interface ITestService
    {
        Task<TestDto> CreateTemplateAsync(
            string name,
            int countOfQuestions,
            int topicId,
            bool isPrivate,
            bool isRandom,
            string? testType
        );
        Task<TestDto> UpdateTestAsync(
            int id,
            string newName,
            int countOfQuestions,
            int? topicId,
            bool isPrivate,
            bool isRandom,
            string? testType
        );

        // Получить шаблон
        Task<TestDto?> GetTestByIdAsync(int id);
        Task<IEnumerable<TestDto>> GetAllTestsAsync();


        // Удалить
        Task DeleteTestAsync(int id);

        Task<PaginatedResponse<TestDto>> GetAllTestsPaginatedAsync(int page, int pageSize, string? currentUserId, bool isAdmin);

        Task<TestDto> AddQuestionToTest(int testId, int questionId);
        Task<TestDto> RemoveQuestionFromTest(int testId, int questionId);
        Task<List<QuestionDto>> GetQuestionsByTestId(int testId);
        Task<List<QuestionDto>> GetTestQuestionsAsync(int testId);
    }
}

