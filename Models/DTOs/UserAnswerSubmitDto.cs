namespace QuizMasterAPI.Models.DTOs
{
    public class UserAnswerSubmitDto
    {
        public int UserTestQuestionId { get; set; }
        // Для обычных вопросов (SingleChoice/MultipleChoice)
        public List<int> SelectedAnswerOptionIds { get; set; } = new();

        // Для текстового вопроса (OpenText)
        public string? UserTextAnswer { get; set; }
    }
}
