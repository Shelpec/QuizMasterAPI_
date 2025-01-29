namespace QuizMasterAPI.Models.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int? TopicId { get; set; } // Include Topic ID
        public string? Topic { get; set; } // Include Topic Name
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
