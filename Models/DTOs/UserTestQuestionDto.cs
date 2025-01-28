namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для передачи данных о вопросе в рамках конкретного UserTest.
    /// </summary>
    public class UserTestQuestionDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }

        // Новое поле: текст вопроса
        public string QuestionText { get; set; } = string.Empty;

        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
