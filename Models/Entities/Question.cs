using System.Collections.Generic;

namespace QuizMasterAPI.Models.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        // Топик (если нужен)
        public int? TopicId { get; set; }
        public Topic? Topic { get; set; }

        // Список вариантов ответов
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    }
}
