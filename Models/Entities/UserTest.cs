using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuizMasterAPI.Models.Entities; // или ваш неймспейс, где лежат User и Test

namespace QuizMasterAPI.Models.Entities
{
    public class UserTest
    {
        [Key]
        public int Id { get; set; }

        // Внешний ключ на User
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        // Внешний ключ на Test
        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        // Количество вопросов
        public int TotalQuestions { get; set; }

        // Количество правильных ответов
        public int CorrectAnswers { get; set; }

        // Дата создания записи
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Флаг, прошёл ли пользователь тест (по желанию)
        public bool IsPassed { get; set; }
    }
}
