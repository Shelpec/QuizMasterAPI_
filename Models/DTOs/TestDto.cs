// Models/DTOs/TestDto.cs
namespace QuizMasterAPI.Models.DTOs
{
    public class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;          // <-- Новые поля
        public int CountOfQuestions { get; set; }
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }                    // <-- Для удобства (Topic?.Name)
        public DateTime CreatedAt { get; set; }
    }
}
