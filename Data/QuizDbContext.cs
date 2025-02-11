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
    public DbSet<UserTestAnswer> UserTestAnswers { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    public DbSet<TestAccess> TestAccesses { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<TestQuestion> TestQuestions { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Важно для Identity

        // Пример: каскадное удаление для AnswerOption
        modelBuilder.Entity<AnswerOption>()
            .HasOne<Question>()
            .WithMany(q => q.AnswerOptions)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserTestQuestion -> связь
        modelBuilder.Entity<UserTestQuestion>()
            .HasOne(utq => utq.UserTest)
            .WithMany(ut => ut.UserTestQuestions)
            .HasForeignKey(utq => utq.UserTestId);

        modelBuilder.Entity<UserTestQuestion>()
            .HasOne(utq => utq.Question)
            .WithMany()
            .HasForeignKey(utq => utq.QuestionId);

        // UserTestAnswer -> связь
        modelBuilder.Entity<UserTestAnswer>()
            .HasOne(uta => uta.UserTestQuestion)
            .WithMany(utq => utq.UserTestAnswers)
            .HasForeignKey(uta => uta.UserTestQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

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
            .WithMany()
            .HasForeignKey(q => q.TopicId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TestAccess>()
            .HasOne(ta => ta.Test)
            .WithMany()
            .HasForeignKey(ta => ta.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Topic>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Test>()
            .Property(t => t.TestType)
            .HasConversion<string>(); // Храним как string

        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Test)
            .WithMany(t => t.TestQuestions)
            .HasForeignKey(tq => tq.TestId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TestQuestion>()
            .HasOne(tq => tq.Question)
            .WithMany()
            .HasForeignKey(tq => tq.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
