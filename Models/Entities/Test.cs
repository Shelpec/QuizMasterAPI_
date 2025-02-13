using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Models.Entities
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(Topic))]
        public int? TopicId { get; set; }
        public Topic? Topic { get; set; }

        public int CountOfQuestions { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPrivate { get; set; } = false;

        public bool IsRandom { get; set; } = false;
        public TestTypeEnum TestType { get; set; } = TestTypeEnum.QuestionsOnly;

        // Связь с TestQuestion
        public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();

        // ➡️ Новое поле: Ограничение по времени (в минутах).
        public int? TimeLimitMinutes { get; set; }
    }
}
