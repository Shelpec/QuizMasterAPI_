namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// DTO для проверки ответов по тесту.
    /// </summary>
    public class TestCheckDto
    {
        public List<TestAnswerValidationDto> Answers { get; set; } = new();
    }

    public class TestAnswerValidationDto
    {
        public int QuestionId { get; set; }
        /// <summary>
        /// Список выбранных ID вариантов ответов (если несколько ответов).
        /// </summary>
        public List<int> SelectedAnswerIds { get; set; } = new();
    }
}
