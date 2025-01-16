using QuizMasterAPI.Models.DTOs;

public class AnswerCheckResultDto
{
    public string QuestionText { get; set; } = string.Empty; // Текст вопроса
    public string CorrectAnswerText { get; set; } = string.Empty; // Текст правильного ответа
    public string SelectedAnswerText { get; set; } = string.Empty; // Текст выбранного пользователем ответа
    public bool IsCorrect { get; set; } // Флаг, правильный ли ответ
}
