namespace QuizMasterAPI.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
        public int CorrectAnswerId { get; set; }
    }

    public class AnswerOption
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int QuestionId { get; set; } // Внешний ключ
    }

}
