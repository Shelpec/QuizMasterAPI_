using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    /// <summary>
    /// Шаблон теста.
    /// Может быть публичным (IsPrivate=false) или приватным (IsPrivate=true).
    /// Если приватный — доступ имеют только пользователи из таблицы TestAccess.
    /// </summary>
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

        /// <summary>
        /// Признак приватности. Если true — тест невидим всем, кроме тех, кто в TestAccess.
        /// Если false — тест публичен, виден всем (или хотя бы всем залогиненным).
        /// </summary>
        public bool IsPrivate { get; set; } = false;
    }
}
