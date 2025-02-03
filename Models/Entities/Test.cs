using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    /// <summary>
    /// Шаблон теста.
    /// Хранит тему (TopicId) и количество вопросов.
    /// Пользователь может выбрать этот шаблон, чтобы пройти тест.
    /// </summary>
    public class Test
    {
        [Key]
        public int Id { get; set; }

        // Название теста (например, "Тест по математике")
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(Topic))]
        public int? TopicId { get; set; }
        public Topic? Topic { get; set; }

        // Количество вопросов, которые нужно сгенерировать при прохождении
        public int CountOfQuestions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
