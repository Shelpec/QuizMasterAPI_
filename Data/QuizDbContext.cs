using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Models.Entities;

public class QuizDbContext : IdentityDbContext<User>
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<UserTest> UserTests { get; set; } = null!;
    public DbSet<UserTestQuestion> UserTestQuestions { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Важно для Identity
                                            // Настройка каскадного удаления для ответа
        modelBuilder.Entity<AnswerOption>()
            .HasOne<Question>()
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Пример настройки связи (можно дополнительно конфигурировать DeleteBehavior и т.д.)
        modelBuilder.Entity<UserTestQuestion>()
            .HasOne(utq => utq.UserTest)
            .WithMany(ut => ut.UserTestQuestions)
            .HasForeignKey(utq => utq.UserTestId);

        modelBuilder.Entity<UserTestQuestion>()
            .HasOne(utq => utq.Question)
            .WithMany()
            .HasForeignKey(utq => utq.QuestionId);

        modelBuilder.Entity<UserTest>()
            .HasOne(ut => ut.User)
            .WithMany()
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserTest>()
            .HasOne(ut => ut.Test)
            .WithMany()
            .HasForeignKey(ut => ut.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.Topic)
            .WithMany()  // если у Topic нет списка вопросов
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.SetNull);

    }
}
