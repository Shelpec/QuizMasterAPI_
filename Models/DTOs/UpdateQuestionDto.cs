namespace QuizMasterAPI.Models.DTOs
{
    public class UpdateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public int CorrectAnswerId { get; set; }
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
