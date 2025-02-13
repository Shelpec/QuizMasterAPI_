using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizMasterAPI.Models.Entities
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Связь: один Category -> много Topics
        // Для этого нужно убедиться, что в БД есть таблица Categories
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
