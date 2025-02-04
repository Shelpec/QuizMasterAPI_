// Допустим, в папке Models/DTOs создадим UserTestHistoryDto.cs

namespace QuizMasterAPI.Models.DTOs
{
    public class UserTestHistoryDto
    {
        // Информация о UserTest
        public int UserTestId { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsPassed { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }

        // Информация о пользователе
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserFullName { get; set; }

        // Информация о тесте
        public int TestId { get; set; }
        public string? TestName { get; set; }
        public int TestCountOfQuestions { get; set; }
        public string? TopicName { get; set; }

        // Детализация по вопросам и выбранным ответам
        public List<QuestionHistoryDto> Questions { get; set; } = new();
    }

    public class QuestionHistoryDto
    {
        public int UserTestQuestionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        // Список вариантов + отмечено ли пользователем, и правильно ли
        public List<AnswerHistoryDto> Answers { get; set; } = new();
    }

    public class AnswerHistoryDto
    {
        public int AnswerOptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }     // правильный ли вариант по базе
        public bool IsChosen { get; set; }      // выбрал ли пользователь
    }
}
