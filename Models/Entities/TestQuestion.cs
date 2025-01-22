using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class TestQuestion
    {

        public int Id { get; set; }

        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }

        [ForeignKey(nameof(Question))]
        public int QuestionId { get; set; }

        public Test Test { get; set; } = null!;
        public Question Question { get; set; } = null!;
    }
}
