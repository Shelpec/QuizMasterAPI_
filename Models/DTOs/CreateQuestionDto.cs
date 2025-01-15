using System.ComponentModel.DataAnnotations;

namespace QuizMasterAPI.Models.DTOs
{
    public class CreateQuestionDto
    {
        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(255, ErrorMessage = "Question text cannot exceed 255 characters.")]
        public string Text { get; set; }

        [Required(ErrorMessage = "CorrectAnswerId is required.")]
        public int CorrectAnswerId { get; set; }

        [Required(ErrorMessage ="Answer options are required.")]
        [MinLength(2, ErrorMessage = "A question must have at least 2 answer options.")]
        [MaxLength(5, ErrorMessage = "A question can have at most 5 answer options.")]
        public List<AnswerOptionDto> AnswerOptions { get; set; }
    }

}
