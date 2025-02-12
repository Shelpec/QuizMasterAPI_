using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для передачи данных о вопросе в рамках конкретного UserTest.
    /// </summary>
    public class UserTestQuestionDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionTypeEnum QuestionType { get; set; }
        public string? CorrectTextAnswer { get; set; } 
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
