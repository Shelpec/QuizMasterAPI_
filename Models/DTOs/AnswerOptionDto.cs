namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для передачи данных о варианте ответа.
    /// </summary>
    public class AnswerOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
