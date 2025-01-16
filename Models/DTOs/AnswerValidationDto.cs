namespace QuizMasterAPI.Models.DTOs
{
    public class AnswerValidationDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new(); // Поддержка нескольких вариантов
    }
}
