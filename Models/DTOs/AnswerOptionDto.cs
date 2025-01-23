using System.Text.Json.Serialization;

namespace QuizMasterAPI.Models.DTOs
{
    public class AnswerOptionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        //[JsonIgnore]
        public bool IsCorrect { get; set; }
    }

}
