// Models/DTOs/TopicDto.cs
namespace QuizMasterAPI.Models.DTOs
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CreateTopicDto
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTopicDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
