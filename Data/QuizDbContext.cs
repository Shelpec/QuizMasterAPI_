﻿using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Data
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options) : base(options) { }

        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<AnswerOption> AnswerOptions { get; set; } = null!;

        public DbSet<Test> Tests { get; set; } = null!;
        public DbSet<TestQuestion> TestQuestions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
        }

    }
}
