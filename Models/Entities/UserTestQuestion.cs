using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QuizMasterAPI.Models.Entities
{
    /// <summary>
    /// Какие конкретно вопросы достались пользователю в рамках его UserTest.
    /// Раньше это могло называться TestQuestion, теперь это привязано именно к UserTest.
    /// </summary>
    public class UserTestQuestion
    {
        [Key]
        public int Id { get; set; }
        
        
        public int UserTestId { get; set; }

        [JsonIgnore]
        public UserTest UserTest { get; set; } = null!;

        // Ссылка на сам вопрос
        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
    }
}
