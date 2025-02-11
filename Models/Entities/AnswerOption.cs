namespace QuizMasterAPI.Models.Entities
{
    public class AnswerOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        // Внешний ключ на Question
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; } // true/false. Если опрос, можно просто ставить false

        // public Question Question { get; set; } // Можно добавить, если нужно навигационное свойство
    }
}
