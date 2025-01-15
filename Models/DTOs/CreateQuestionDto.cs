namespace QuizMasterAPI.Models.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
