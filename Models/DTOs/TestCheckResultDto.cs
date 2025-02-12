namespace QuizMasterAPI.Models.DTOs
{
    public class TestCheckResultDto
    {
        public int CorrectCount { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionCheckResultDto> Results { get; set; } = new();
    }

    public class QuestionCheckResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public string? CorrectTextAnswer { get; set; } // Если это текстовый ответ
        public List<string> CorrectAnswers { get; set; } = new();
        public List<string> SelectedAnswers { get; set; } = new();
    }
}
