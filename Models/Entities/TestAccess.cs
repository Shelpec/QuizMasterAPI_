using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    /// <summary>
    /// Запись о том, что пользователь UserId имеет доступ к приватному тесту TestId.
    /// Если тест публичный (IsPrivate=false), то эта таблица не влияет.
    /// </summary>
    public class TestAccess
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }
        public Test Test { get; set; } = null!;

        /// <summary>
        /// Ссылка на пользователя (AspNetUsers.Id)
        /// </summary>
        public string UserId { get; set; } = null!;

        // Можно хранить объект User, если нужно
        // [ForeignKey(nameof(UserId))]
        // public User? User { get; set; }
    }
}
