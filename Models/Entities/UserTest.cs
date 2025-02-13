using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class UserTest
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!;

        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public bool IsPassed { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // ✅ Новые поля для времени
        public DateTime? StartTime { get; set; }   // Когда пользователь начал
        public DateTime? ExpireTime { get; set; } // Когда тест истекает
        public DateTime? EndTime { get; set; }     // Когда реально закончил
        public int? TimeSpentSeconds { get; set; } // Сколько секунд заняло

        public ICollection<UserTestQuestion> UserTestQuestions { get; set; } = new List<UserTestQuestion>();
    }
}
