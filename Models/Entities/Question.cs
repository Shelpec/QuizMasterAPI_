using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Models.Entities
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        // Связь с топиком
        public int? TopicId { get; set; }
        public Topic? Topic { get; set; }

        // Список вариантов ответов
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();

        //  Добавлен новый тип вопроса
        public QuestionTypeEnum QuestionType { get; set; } = QuestionTypeEnum.SingleChoice;

        //  Добавлено поле для текстового ответа (если вопрос типа OpenText)
        public string? CorrectTextAnswer { get; set; }
    }
}
