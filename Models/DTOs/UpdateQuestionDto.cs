namespace QuizMasterAPI.Models.DTOs
{
    public class UpdateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
