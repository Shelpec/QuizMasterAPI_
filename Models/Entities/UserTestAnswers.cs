using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class UserTestAnswer
    {
        [Key]
        public int Id { get; set; }

        // Связь на UserTestQuestion
        public int UserTestQuestionId { get; set; }
        public UserTestQuestion UserTestQuestion { get; set; } = null!;

        // Какой вариант ответа выбрал пользователь
        public int AnswerOptionId { get; set; }
        // Можно добавить навигацию:
        // public AnswerOption? AnswerOption { get; set; }
    }
}
