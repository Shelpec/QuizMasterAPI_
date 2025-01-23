namespace QuizMasterAPI.Models.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;    // Email пользователя
        public string Password { get; set; } = string.Empty; // Пароль пользователя
    }
}
