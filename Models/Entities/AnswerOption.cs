namespace QuizMasterAPI.Models.Entities
{
    public class AnswerOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; } 
    }
}
