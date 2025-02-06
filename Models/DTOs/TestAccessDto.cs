namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для отображения доступа к приватному тесту.
    /// </summary>
    public class TestAccessDto
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
