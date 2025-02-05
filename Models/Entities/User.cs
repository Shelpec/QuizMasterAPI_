using Microsoft.AspNetCore.Identity;

namespace QuizMasterAPI.Models.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
    }
}
