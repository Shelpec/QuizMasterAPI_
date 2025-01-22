namespace QuizMasterAPI.Models.DTOs
{
    public class UpdateTestDto
    {
        public List<int> QuestionIds { get; set; } = new();
    }
}
