using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Data
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>().HasKey(q => q.Id);
            modelBuilder.Entity<AnswerOption>().HasKey(a => a.Id);
        }
    }
}
