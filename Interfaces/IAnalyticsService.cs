using QuizMasterAPI.Models.DTOs;

public interface IAnalyticsService
{
    Task<TestAnalyticsDto> GetTestAnalyticsAsync(int testId);

    // Новый метод, который вернет и аналитику, и историю
    Task<TestAnalyticsWithHistoryDto> GetTestAnalyticsAndHistoryAsync(int testId);

    /// <summary>
    /// Сколько раз проходили данный тест (общее число попыток).
    /// </summary>
    Task<int> GetTotalAttemptsAsync(int testId);

    /// <summary>
    /// Средний балл (0..100), за всё время.
    /// </summary>
    Task<double> GetAverageScoreAsync(int testId);

    /// <summary>
    /// Распределение баллов: например, сегменты "80-100%" -> 50 чел, "60-79%" -> 60 чел, и т.д.
    /// </summary>
    Task<List<ScoreRangeDto>> GetScoreDistributionAsync(int testId);

    /// <summary>
    /// Возвращает TOP-5 (или N) самых сложных вопросов, т.е. на которые чаще всего отвечают неправильно.
    /// </summary>
    Task<List<HardQuestionDto>> GetHardestQuestionsAsync(int testId, int top);

    /// <summary>
    /// Возвращает TOP-5 (или N) лучших участников, с указанием их балла и времени прохождения.
    /// </summary>
    Task<List<TopPlayerDto>> GetTopPlayersAsync(int testId, int top);
}
