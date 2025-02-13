using System.Collections.Generic;

namespace QuizMasterAPI.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        // Название категории
        public string Name { get; set; }

        public ICollection<Topic> Topics { get; set; }
    }
}
