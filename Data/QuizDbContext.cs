using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Models;

namespace QuizMasterAPI.Data
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Настройка автоинкремента для Id в AnswerOption
            modelBuilder.Entity<AnswerOption>()
                .Property(a => a.Id)
                .ValueGeneratedOnAdd();

            // Сеянс для вопросов
            modelBuilder.Entity<Question>().HasData(
                new Question
                {
                    Id = 1,
                    Text = "What is the capital of France?",
                    CorrectAnswerId = 2 // Id правильного ответа
                });

            // Сеянс для вариантов ответов
            modelBuilder.Entity<AnswerOption>().HasData(
                new AnswerOption { Id = 1, Text = "Berlin", QuestionId = 1 },
                new AnswerOption { Id = 2, Text = "Paris", QuestionId = 1 },
                new AnswerOption { Id = 3, Text = "Rome", QuestionId = 1 });

            base.OnModelCreating(modelBuilder);
        }



    }
}
