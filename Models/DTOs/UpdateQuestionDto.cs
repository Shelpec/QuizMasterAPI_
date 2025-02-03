using QuizMasterAPI.Models.DTOs;

public class UpdateQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public int? TopicId { get; set; }           // <-- новое поле
    public List<AnswerOptionDto> AnswerOptions { get; set; } = new();
}
