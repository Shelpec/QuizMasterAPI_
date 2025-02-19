namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// Пример: "80-100%" -> 50 чел
    /// </summary>
    public class ScoreRangeDto
    {
        public string RangeLabel { get; set; } = string.Empty; // "80-100%"
        public int Count { get; set; }
    }

    /// <summary>
    /// Самые сложные (hard) вопросы
    /// </summary>
    public class HardQuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        /// <summary>
        /// Процент правильных ответов (0..100)
        /// </summary>
        public double CorrectPercentage { get; set; }
        /// <summary>
        /// Общее число попыток ответа
        /// </summary>
        public int AttemptsCount { get; set; }
    }

    /// <summary>
    /// Лучшие игроки
    /// </summary>
    public class TopPlayerDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        /// <summary>
        /// Например, "95%" или 95.0
        /// </summary>
        public double ScorePercent { get; set; }
        /// <summary>
        /// Время прохождения (например, "5:10" )
        /// </summary>
        public string TimeSpentFormatted { get; set; } = string.Empty;
    }
}
