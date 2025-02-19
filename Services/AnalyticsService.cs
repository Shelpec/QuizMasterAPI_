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


        public async Task<int> GetTotalAttemptsAsync(int testId)
        {
            // Считаем, сколько записей UserTest есть с данным testId
            int count = await _ctx.UserTests
                .Where(ut => ut.TestId == testId)
                .CountAsync();
            return count;
        }

        public async Task<double> GetAverageScoreAsync(int testId)
        {
            // Предположим, у нас в UserTest есть CorrectAnswers, TotalQuestions
            // И мы считаем (CorrectAnswers / TotalQuestions)*100
            // Либо мы можем смотреть в UserTestAnswers

            var list = await _ctx.UserTests
                .Where(ut => ut.TestId == testId && ut.TotalQuestions > 0)
                .Select(ut => new
                {
                    Score = (double)ut.CorrectAnswers / ut.TotalQuestions * 100.0
                })
                .ToListAsync();

            if (list.Count == 0) return 0.0;

            double avg = list.Average(x => x.Score);
            return avg; // например, 76.123
        }

        public async Task<List<ScoreRangeDto>> GetScoreDistributionAsync(int testId)
        {
            // Допустим, хотим 4 сегмента:
            // 80-100, 60-79, 40-59, 0-39
            // Считаем по UserTests

            var userTests = await _ctx.UserTests
                .Where(ut => ut.TestId == testId && ut.TotalQuestions > 0)
                .Select(ut => new
                {
                    Score = (double)ut.CorrectAnswers / ut.TotalQuestions * 100.0
                })
                .ToListAsync();

            int c80 = userTests.Count(x => x.Score >= 80);
            int c60 = userTests.Count(x => x.Score >= 60 && x.Score < 80);
            int c40 = userTests.Count(x => x.Score >= 40 && x.Score < 60);
            int c0 = userTests.Count(x => x.Score < 40);

            var dist = new List<ScoreRangeDto>
            {
                new ScoreRangeDto { RangeLabel = "80-100%", Count = c80 },
                new ScoreRangeDto { RangeLabel = "60-79%",  Count = c60 },
                new ScoreRangeDto { RangeLabel = "40-59%",  Count = c40 },
                new ScoreRangeDto { RangeLabel = "0-39%",   Count = c0 },
            };

            return dist;
        }

        public async Task<List<HardQuestionDto>> GetHardestQuestionsAsync(int testId, int top)
        {
            // Предположим, для определения "сложных" вопросов мы смотрим UserTestAnswers
            // Сгруппируем по QuestionId, считаем кол-во правильных / общее.

            // Нужно связать UserTest -> TestId, UserTestQuestions -> QuestionId, 
            // и посмотреть корректность...

            var query = await (
                from ut in _ctx.UserTests
                join utq in _ctx.UserTestQuestions on ut.Id equals utq.UserTestId
                join q in _ctx.Questions on utq.QuestionId equals q.Id
                where ut.TestId == testId && ut.TotalQuestions > 0
                select new
                {
                    q.Id,
                    q.Text,
                    IsCorrect = true // например, если мы флаг храним
                }
            ).ToListAsync();

            // но часто бывает, что IsCorrect хранится в UserTestAnswers
            // Здесь псевдокод, возможно, в вашей схеме иначе.

            // Группируем
            var group = query
                .GroupBy(x => new { x.Id, x.Text })
                .Select(g => new
                {
                    QuestionId = g.Key.Id,
                    QuestionText = g.Key.Text,
                    Attempts = g.Count(),
                    Correct = g.Count(x => x.IsCorrect) // или другое поле
                })
                .ToList();

            // Считаем процент
            var list = group
                .Select(x => new HardQuestionDto
                {
                    QuestionId = x.QuestionId,
                    QuestionText = x.QuestionText,
                    AttemptsCount = x.Attempts,
                    CorrectPercentage = x.Attempts > 0
                        ? (double)x.Correct / x.Attempts * 100.0
                        : 0.0
                })
                // сортируем по возрастанию процент правильных, т.е. самые низкие -> "самые сложные"
                .OrderBy(h => h.CorrectPercentage)
                .Take(top)
                .ToList();

            return list;
        }

        public async Task<List<TopPlayerDto>> GetTopPlayersAsync(int testId, int top)
        {
            // Ищем пользователей, у которых наивысший процент.
            // UserTests => (CorrectAnswers / TotalQuestions)*100

            var query = await (
                from ut in _ctx.UserTests
                join user in _ctx.Users on ut.UserId equals user.Id
                where ut.TestId == testId && ut.TotalQuestions > 0
                select new
                {
                    ut.UserId,
                    user.FullName,
                    Score = (double)ut.CorrectAnswers / ut.TotalQuestions * 100.0,
                    // Предположим, храним TimeSpentSeconds
                    ut.TimeSpentSeconds
                }
            ).ToListAsync();

            var topList = query
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.TimeSpentSeconds) // например, кто быстрее при одинаковом балле
                .Take(top)
                .Select(x => new TopPlayerDto
                {
                    UserId = x.UserId,
                    UserFullName = x.FullName ?? x.UserId,
                    ScorePercent = x.Score,
                    TimeSpentFormatted = FormatTime(x.TimeSpentSeconds)
                })
                .ToList();

            return topList;
        }

        private string FormatTime(int? secs)
        {
            if (!secs.HasValue) return "--:--";
            TimeSpan ts = TimeSpan.FromSeconds(secs.Value);
            // Например, "mm:ss" 
            return $"{(int)ts.TotalMinutes}:{ts.Seconds:00}";
        }
    }
}
