using System.Collections.Generic;

namespace QuizMasterAPI.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        // Название категории
        public string Name { get; set; }

        // При желании можете добавить поле Description, если нужно
        // public string Description { get; set; }

        // Навигационное свойство — список топиков (будет позже)
        public ICollection<Topic> Topics { get; set; }
    }
}
