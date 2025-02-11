namespace QuizMasterAPI.Models.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public int? TopicId { get; set; }  // если хотим привязать к топику
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
