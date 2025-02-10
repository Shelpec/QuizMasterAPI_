namespace QuizMasterAPI.Models.DTOs
{
    public class TopicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public bool IsSurveyTopic { get; set; } // <-- новое поле
    }

    public class CreateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSurveyTopic { get; set; } = false;
    }

    public class UpdateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsSurveyTopic { get; set; } = false;
    }
}
