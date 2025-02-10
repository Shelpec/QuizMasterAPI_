namespace QuizMasterAPI.Models.DTOs
{
    public class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public int CountOfQuestions { get; set; }
        public DateTime CreatedAt { get; set; }

        // Новое поле
        public bool IsPublic { get; set; }
        public bool IsSurvey { get; set; }
    }
}
