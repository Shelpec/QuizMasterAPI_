namespace QuizMasterAPI.Models.DTOs
{
    public class TestQuestionDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public int QuestionId { get; set; }
        public QuestionDto Question { get; set; } = new();
    }
}
