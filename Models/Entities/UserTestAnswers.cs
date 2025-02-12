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

        // Если это обычный вопрос, то хранится выбранный AnswerOptionId
        public int? AnswerOptionId { get; set; }

        // Если это вопрос с открытым ответом (OpenText),
        // то здесь будет храниться введённый пользователем текст.
        public string? UserTextAnswer { get; set; }
    }
}
