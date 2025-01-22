using QuizMasterAPI.Models.DTOs;

public class TestQuestionDto
{
    public int Id { get; set; }

    // Вместо отдельных полей QuestionId / QuestionText,
    // используем полноценный QuestionDto:
    public QuestionDto Question { get; set; } = new();
}
