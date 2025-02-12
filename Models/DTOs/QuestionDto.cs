using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Enums;

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public QuestionTypeEnum QuestionType { get; set; } // Показываем тип вопроса
    public string? CorrectTextAnswer { get; set; } // Для OpenText
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
}
