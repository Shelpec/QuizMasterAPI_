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

        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserFullName { get; set; }

        public int TestId { get; set; }
        public string? TestName { get; set; }
        public int TestCountOfQuestions { get; set; }
        public string? TopicName { get; set; }
        public List<QuestionHistoryDto> Questions { get; set; } = new();
    }

    public class QuestionHistoryDto
    {
        public int UserTestQuestionId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionTypeEnum QuestionType { get; set; } 
        public string? CorrectTextAnswer { get; set; } 
        public List<AnswerHistoryDto> AnswerOptions { get; set; } = new();
    }

    public class AnswerHistoryDto
    {
        public int AnswerOptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public bool IsChosen { get; set; }
        public string? UserTextAnswer { get; set; } // Новое поле для хранения текста
    }

}
