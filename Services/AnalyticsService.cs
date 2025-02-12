using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Enums;
using System;
using System.Linq;

namespace QuizMasterAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly QuizDbContext _ctx;
        private readonly IUserTestService _userTestService;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            QuizDbContext ctx,
            IUserTestService userTestService,
            ILogger<AnalyticsService> logger)
        {
            _ctx = ctx;
            _userTestService = userTestService;
            _logger = logger;
        }

        /// <summary>
        /// Аналитика по тесту (средний балл, сложные вопросы).
        /// </summary>
        public async Task<TestAnalyticsDto> GetTestAnalyticsAsync(int testId)
        {
            _logger.LogInformation("GetTestAnalyticsAsync for TestId={TestId}", testId);

            // 1) Проверяем, есть ли тест
            var test = await _ctx.Tests
                .Include(t => t.Topic)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                throw new KeyNotFoundException($"Test with ID={testId} not found");

            // 2) Берём все UserTests (попытки прохождения)
            var userTests = await _ctx.UserTests
                .Where(ut => ut.TestId == testId)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                .ToListAsync();

            // Общее число попыток (UserTest)
            var totalAttempts = userTests.Count;

            // Если никто не проходил — возвращаем пустую статистику
            if (totalAttempts == 0)
            {
                return new TestAnalyticsDto
                {
                    TestId = testId,
                    TestName = test.Name,
                    AverageScorePercent = 0,
                    TotalAttempts = 0,
                    AverageTimeSeconds = 0,
                    DifficultQuestions = new List<DifficultQuestionDto>()
                };
            }

            // 3) Средний балл (correctAnswers / totalQuestions)
            double sumScorePercent = 0;
            double sumDurationSeconds = 0; // Пока не используем

            foreach (var ut in userTests)
            {
                if (ut.TotalQuestions == 0) continue;

                double percent = (double)ut.CorrectAnswers / ut.TotalQuestions * 100.0;
                sumScorePercent += percent;
                // sumDurationSeconds += ... (если бы мы считали время прохождения)
            }

            double averageScorePercent = sumScorePercent / totalAttempts;
            double averageTimeSeconds = 0; // (т.к. не реализовано)

            // 4) "Самые трудные вопросы"
            //    Для каждого вопроса считаем, в скольких попытках он был отвечен неверно.
            //    incorrectCount для вопроса / totalAttempts * 100 => WrongRatePercent
            //
            // questionStats: questionId -> (incorrectCount, questionText)
            var questionStats = new Dictionary<int, (int incorrectCount, string questionText)>();

            // Проходим все прохождения
            foreach (var ut in userTests)
            {
                // Для каждого вопроса смотрим, верно ли отвечен
                foreach (var utq in ut.UserTestQuestions)
                {
                    var q = utq.Question;
                    if (q == null) continue;

                    // Для OpenText / Survey / SingleChoice / MultipleChoice:
                    // Простой способ проверки (без "частичного" подсчета):
                    var correctIds = q.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    var chosenIds = utq.UserTestAnswers
                        .Where(a => a.AnswerOptionId.HasValue)
                        .Select(a => a.AnswerOptionId!.Value)
                        .ToList();

                    bool isCorrect = false;
                    if (q.QuestionType == QuestionTypeEnum.Survey)
                    {
                        isCorrect = true;
                    }
                    else if (q.QuestionType == QuestionTypeEnum.OpenText)
                    {
                        // Допустим, считаем всегда неверно?
                        isCorrect = false;
                    }
                    else
                    {
                        // SingleChoice / MultipleChoice
                        isCorrect = !correctIds.Except(chosenIds).Any()
                                    && !chosenIds.Except(correctIds).Any();
                    }

                    // Если нет записи о вопросе — добавляем
                    if (!questionStats.ContainsKey(q.Id))
                    {
                        questionStats[q.Id] = (0, q.Text);
                    }

                    // Если неверно отвечен => инкремент
                    if (!isCorrect)
                    {
                        var old = questionStats[q.Id];
                        questionStats[q.Id] = (old.incorrectCount + 1, old.questionText);
                    }
                }
            }

            // 5) Вычисляем WrongRatePercent = (incorrectCount / totalAttempts) * 100
            //    => берем топ 3
            var diffQ = questionStats
                .Select(x => new DifficultQuestionDto
                {
                    QuestionId = x.Key,
                    QuestionText = x.Value.questionText,
                    WrongRatePercent = (double)x.Value.incorrectCount / totalAttempts * 100.0
                })
                .OrderByDescending(d => d.WrongRatePercent)
                .Take(3)
                .ToList();

            // 6) Собираем результат
            var resultDto = new TestAnalyticsDto
            {
                TestId = testId,
                TestName = test.Name,
                AverageScorePercent = averageScorePercent,
                TotalAttempts = totalAttempts,
                AverageTimeSeconds = averageTimeSeconds,
                DifficultQuestions = diffQ
            };

            return resultDto;
        }

        /// <summary>
        /// Возвращаем аналитику + историю прохождений
        /// </summary>
        public async Task<TestAnalyticsWithHistoryDto> GetTestAnalyticsAndHistoryAsync(int testId)
        {
            _logger.LogInformation("GetTestAnalyticsAndHistoryAsync for TestId={TestId}", testId);

            // 1) Аналитика
            var analytics = await GetTestAnalyticsAsync(testId);

            // 2) История (кто и как прошёл)
            var historyList = await _userTestService.GetAllHistoryByTestId(testId);

            // 3) Склеиваем
            var result = new TestAnalyticsWithHistoryDto
            {
                TestId = analytics.TestId,
                TestName = analytics.TestName,
                AverageScorePercent = analytics.AverageScorePercent,
                TotalAttempts = analytics.TotalAttempts,
                AverageTimeSeconds = analytics.AverageTimeSeconds,
                DifficultQuestions = analytics.DifficultQuestions,
                History = historyList
            };

            return result;
        }
    }
}
