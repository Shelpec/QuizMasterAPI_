using QuizMasterAPI.Models.Enums;

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

        public bool IsPrivate { get; set; }
        public bool IsRandom { get; set; }
        public TestTypeEnum TestType { get; set; }

        // ✅ Новое поле
        public int? TimeLimitMinutes { get; set; }

        public List<TestQuestionDto> TestQuestions { get; set; } = new();
    }
}
