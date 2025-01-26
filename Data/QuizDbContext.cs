using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Models.Entities;

public class QuizDbContext : IdentityDbContext<User>
{
    public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;
    public DbSet<Test> Tests { get; set; } = null!;
    public DbSet<TestQuestion> TestQuestions { get; set; } = null!;


    public DbSet<UserTest> UserTests { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Важно для Identity
                                            // Настройка каскадного удаления для ответа
        modelBuilder.Entity<AnswerOption>()
            .HasOne<Question>()
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Настройка связей TestQuestion -> Test, TestQuestion -> Question
        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Test)
            .WithMany(t => t.TestQuestions)
            .HasForeignKey(tq => tq.TestId);

        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Question)
            .WithMany()
            .HasForeignKey(tq => tq.QuestionId);

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
    }
}
