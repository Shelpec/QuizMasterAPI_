using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class Test
    {

        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Связь с таблицей TestQuestion
        public ICollection<TestQuestion> TestQuestions { get; set; } = new List<TestQuestion>();
    }
}
