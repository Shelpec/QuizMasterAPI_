namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для передачи ответов на вопросы.
    /// </summary>
    public class TestAnswerValidationDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedAnswerIds { get; set; } = new();
    }

    /// <summary>
    /// Результат проверки одного целого теста.
    /// </summary>


    /// <summary>
    /// Детализация по каждому вопросу.
    /// </summary>

}
