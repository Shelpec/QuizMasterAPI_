using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(Topic))]
        public int? TopicId { get; set; }
        public Topic? Topic { get; set; }

        public int CountOfQuestions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPrivate { get; set; } = false;

        /// <summary>
        /// Новое поле: Если true, то это «опросник», не оцениваем правильность.
        /// </summary>
        public bool IsSurvey { get; set; } = false;
    }
}
