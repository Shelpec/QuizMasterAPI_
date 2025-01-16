namespace QuizMasterAPI.Models.DTOs
{
    public class AnswerValidationResponseDto
    {
        public int CorrectCount { get; set; }
        public List<QuestionValidationResultDto> Results { get; set; } = new();
    }
    public class QuestionValidationResultDto
    {
        public string QuestionText { get; set; } = string.Empty;
        public List<string> CorrectAnswers { get; set; } = new(); // Тексты правильных ответов
        public List<string> SelectedAnswers { get; set; } = new(); // Тексты выбранных ответов
    }
}
