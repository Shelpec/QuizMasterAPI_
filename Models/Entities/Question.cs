using QuizMasterAPI.Models.Entities;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;

    // ForeignKey:
    public int? TopicId { get; set; } // можно сделать int? (nullable), если тема не обязательна
    public Topic? Topic { get; set; } // навигационное свойство

    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
