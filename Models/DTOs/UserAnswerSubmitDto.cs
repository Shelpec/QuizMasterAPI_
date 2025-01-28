namespace QuizMasterAPI.Models.DTOs
{
    public class UserAnswerSubmitDto
    {
        // Какой UserTestQuestion обрабатываем
        public int UserTestQuestionId { get; set; }

        // Какие варианты ответа выбраны
        public List<int> SelectedAnswerOptionIds { get; set; } = new();
    }
}
