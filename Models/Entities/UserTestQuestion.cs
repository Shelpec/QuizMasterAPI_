using QuizMasterAPI.Models.Entities;

public class UserTestQuestion
{
    public int Id { get; set; }

    public int UserTestId { get; set; }
    public UserTest UserTest { get; set; } = null!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public ICollection<UserTestAnswer> UserTestAnswers { get; set; } = new List<UserTestAnswer>();
}
