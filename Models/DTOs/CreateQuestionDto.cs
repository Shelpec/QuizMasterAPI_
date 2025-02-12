using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Models.DTOs
{
    public class CreateQuestionDto
    {
        public string Text { get; set; } = string.Empty;
        public int? TopicId { get; set; }
        public QuestionTypeEnum QuestionType { get; set; } = QuestionTypeEnum.SingleChoice;
        public string? CorrectTextAnswer { get; set; } // Можно задать текстовый ответ
        public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
    }
}
