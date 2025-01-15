public class QuizRequestDto
{
    public int QuestionCount { get; set; }
    public List<int> SelectedAnswers { get; set; } = new List<int>();
}
