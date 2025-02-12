using QuizMasterAPI.Models.DTOs;

public interface IAnalyticsService
{
    Task<TestAnalyticsDto> GetTestAnalyticsAsync(int testId);

    // Новый метод, который вернет и аналитику, и историю
    Task<TestAnalyticsWithHistoryDto> GetTestAnalyticsAndHistoryAsync(int testId);
}
