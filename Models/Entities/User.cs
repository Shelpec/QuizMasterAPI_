using Microsoft.AspNetCore.Identity;

namespace QuizMasterAPI.Models.Entities
{
    public class User : IdentityUser
    {
        // Вы можете добавлять дополнительные поля, например:
        public string FullName { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
