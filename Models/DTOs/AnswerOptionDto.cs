using System.ComponentModel.DataAnnotations;

namespace QuizMasterAPI.Models.DTOs
{
    public class AnswerOptionDto
    {
        public int? Id { get; set; } // ID может быть null при создании нового ответа

        [Required(ErrorMessage = "Answer text is required.")]
        [StringLength(100, ErrorMessage = "Answe text cannot exceed 100 characters.")]
        public string Text { get; set; }
        public int? QuestionId { get; set; } // ID вопроса, если требуется связь


    }
}

