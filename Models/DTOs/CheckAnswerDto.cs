namespace QuizMasterAPI.Models.DTOs
{
    public class CheckAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new();
        public string? UserTextAnswer { get; set; } // Добавлено для текстового ответа
    }
}
