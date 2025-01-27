using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    /// <summary>
    /// Фиксирует, какой пользователь (UserId) проходит какой шаблон (TestId),
    /// сколько вопросов было сгенерировано, сколько из них ответил правильно и т.д.
    /// </summary>
    public class UserTest
    {
        [Key]
        public int Id { get; set; }

        // ID пользователя (из Identity)
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;  // Если нужна навигация на User

        // Ссылка на шаблон теста
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        // Сколько вопросов реально добавлено пользователю
        public int TotalQuestions { get; set; }
        // Сколько ответил правильно
        public int CorrectAnswers { get; set; }

        public bool IsPassed { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Связка на UserTestQuestion
        public ICollection<UserTestQuestion> UserTestQuestions { get; set; } = new List<UserTestQuestion>();
    }
}
