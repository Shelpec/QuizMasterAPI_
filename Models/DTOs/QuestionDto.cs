namespace QuizMasterAPI.Models.DTOs
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        public int? TopicId { get; set; }
        public string? TopicName { get; set; } // Показываем имя топика (Topic.Name)

        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
