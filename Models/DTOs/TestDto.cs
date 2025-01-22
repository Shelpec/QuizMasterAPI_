namespace QuizMasterAPI.Models.DTOs
{
    public class TestDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TestQuestionDto> Questions { get; set; } = new();
    }

}
