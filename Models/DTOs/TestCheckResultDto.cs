namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// Результат проверки ответов по тесту.
    /// </summary>
    public class TestCheckResultDto
    {
        public bool IsSurvey { get; set; }  // ← Новое поле
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionCheckResultDto> Results { get; set; } = new();
    }

    public class QuestionCheckResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public List<string> CorrectAnswers { get; set; } = new();
        public List<string> SelectedAnswers { get; set; } = new();
    }
}
