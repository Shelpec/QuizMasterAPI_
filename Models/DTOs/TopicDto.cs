namespace QuizMasterAPI.Models.DTOs
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public bool IsSurveyTopic { get; set; }

        // Новое поле: привязка к категории
        public int CategoryId { get; set; }
    }

    public class CreateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSurveyTopic { get; set; } = false;

        // При создании указываем, в какой категории создаём топик
        public int CategoryId { get; set; }
    }

    public class UpdateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSurveyTopic { get; set; } = false;

        // Позволяем менять категорию (по желанию), если хотите
        public int CategoryId { get; set; }
    }
}
