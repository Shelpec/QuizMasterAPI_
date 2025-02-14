namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для передачи данных о конкретном UserTest (сеансе теста).
    /// </summary>
    public class UserTestDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ExpireTime { get; set; }

        public List<UserTestQuestionDto> UserTestQuestions { get; set; } = new();
    }
}
