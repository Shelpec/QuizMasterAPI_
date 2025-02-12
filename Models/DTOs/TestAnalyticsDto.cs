namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// Общая статистика по одному тесту
    /// </summary>
    public class TestAnalyticsDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; } = string.Empty;

        // Средний балл (процент правильно отвеченных вопросов)
        public double AverageScorePercent { get; set; }

        // Сколько пользователей прошли этот тест
        public int TotalAttempts { get; set; }

        // Среднее время прохождения (в секундах, минутах и т.п.)
        public double AverageTimeSeconds { get; set; }

        // Топ самых сложных вопросов
        public List<DifficultQuestionDto> DifficultQuestions { get; set; } = new();
    }

    /// <summary>
    /// DTO для "сложных" вопросов
    /// </summary>
    public class DifficultQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        // Процент неправильных ответов (или кол-во неудачных)
        public double WrongRatePercent { get; set; }
    }
}
