namespace QuizMasterAPI.Models.DTOs
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        // Если нужно вернуть название категории – можно добавить CategoryName
        // public string? CategoryName { get; set; }
    }

    public class CreateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }

    public class UpdateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
    }
}
