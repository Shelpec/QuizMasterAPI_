using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Models.DTOs
{
    public class UserTestHistoryDto
    {
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

        // Поля времени
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? TimeSpentSeconds { get; set; }

        // Если вам нужно определить, Survey ли
        public bool TopicIsSurvey { get; set; } // при желании

        // Список вопросов
        public List<QuestionHistoryDto> Questions { get; set; } = new();
    }

    public class QuestionHistoryDto
    {
        public int UserTestQuestionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;

        // Можно хранить тип вопроса
        public QuestionTypeEnum QuestionType { get; set; }

        // Список конкретных «ответов»
        public List<AnswerHistoryDto> AnswerOptions { get; set; } = new();
    }

    public class AnswerHistoryDto
    {
        // Для вариантных вопросов
        public int? AnswerOptionId { get; set; }
        public string? Text { get; set; }

        // Показываем, правильно ли этот вариант и был ли выбран
        public bool IsCorrect { get; set; }
        public bool IsChosen { get; set; }

        // Для OpenText, если пользователь вводил текст
        public string? UserTextAnswer { get; set; }
    }
}
