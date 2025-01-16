namespace QuizMasterAPI.Models.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool HasMultipleCorrectAnswers { get; set; } // Новый флаг
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
