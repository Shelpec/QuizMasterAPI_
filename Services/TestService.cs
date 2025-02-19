using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting; // for IWebHostEnvironment
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;
using System.IO;

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<TestService> _logger;
        private readonly IMapper _mapper;
        private readonly QuizDbContext _ctx;
        private readonly IAnalyticsService _analyticsService;
        private readonly ITestQuestionRepository _testQuestionRepository;

        // Even though we have IWebHostEnvironment, we won't use it here for fonts
        private readonly IWebHostEnvironment _env;

        public TestService(
            ITestRepository testRepository,
            ITopicRepository topicRepository,
            IQuestionRepository questionRepository,
            ITestQuestionRepository testQuestionRepository,
            ILogger<TestService> logger,
            IMapper mapper,
            QuizDbContext ctx,
            IAnalyticsService analyticsService,
            IWebHostEnvironment env
        )
        {
            _testRepository = testRepository;
            _topicRepository = topicRepository;
            _questionRepository = questionRepository;
            _testQuestionRepository = testQuestionRepository;
            _logger = logger;
            _mapper = mapper;
            _ctx = ctx;
            _analyticsService = analyticsService;
            _env = env;
        }

        #region Basic CRUD for Test

        public async Task<TestDto> CreateTemplateAsync(
            string name,
            int countOfQuestions,
            int topicId,
            bool isPrivate,
            bool isRandom,
            string? testType,
            int? timeLimitMinutes = null
        )
        {
            var topic = await _topicRepository.GetTopicByIdAsync(topicId);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={topicId} not found");

            var finalTestType = TestTypeEnum.QuestionsOnly;
            if (!string.IsNullOrEmpty(testType))
            {
                if (!Enum.TryParse<TestTypeEnum>(testType, ignoreCase: true, out finalTestType))
                {
                    throw new ArgumentException($"Invalid testType value: {testType}");
                }
            }

            var test = new Test
            {
                Name = name,
                TopicId = topicId,
                Topic = topic,
                CountOfQuestions = countOfQuestions,
                CreatedAt = DateTime.UtcNow,
                IsPrivate = isPrivate,
                IsRandom = isRandom,
                TestType = finalTestType,
                TimeLimitMinutes = timeLimitMinutes
            };

            await _testRepository.AddAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }

        public async Task<TestDto?> GetTestByIdAsync(int id)
        {
            _logger.LogInformation("GetTestByIdAsync(Id={Id})", id);
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                return null;

            return _mapper.Map<TestDto>(test);
        }

        public async Task<IEnumerable<TestDto>> GetAllTestsAsync()
        {
            _logger.LogInformation("GetAllTestsAsync()");
            var tests = await _testRepository.GetAllTestsAsync();
            return _mapper.Map<IEnumerable<TestDto>>(tests);
        }

        public async Task<PaginatedResponse<TestDto>> GetAllTestsPaginatedAsync(
            int page,
            int pageSize,
            string? currentUserId,
            bool isAdmin)
        {
            _logger.LogInformation("GetAllTestsPaginatedAsync(page={Page}, pageSize={Size}, userId={User}, isAdmin={Admin})",
                page, pageSize, currentUserId, isAdmin);

            var query = _ctx.Tests
                .Include(t => t.Topic)
                .AsQueryable();

            // If user is not admin => show only public or permitted tests
            if (!isAdmin && !string.IsNullOrEmpty(currentUserId))
            {
                var testIdsWithAccess = _ctx.TestAccesses
                    .Where(ta => ta.UserId == currentUserId)
                    .Select(ta => ta.TestId)
                    .Distinct();

                query = query.Where(t => t.IsPrivate == false || testIdsWithAccess.Contains(t.Id));
            }

            var totalItems = await query.CountAsync();
            var skip = (page - 1) * pageSize;
            var tests = await query
                .OrderBy(t => t.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var dtos = _mapper.Map<List<TestDto>>(tests);

            return new PaginatedResponse<TestDto>
            {
                Items = dtos,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<TestDto> UpdateTestAsync(
            int id,
            string newName,
            int countOfQuestions,
            int? topicId,
            bool isPrivate,
            bool isRandom,
            string? testType,
            int? timeLimitMinutes = null
        )
        {
            _logger.LogInformation("UpdateTestAsync(Id={Id}, isPrivate={Priv}, isRandom={Rand}, testType={Type})",
                id, isPrivate, isRandom, testType);

            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={id} not found.");

            test.Name = newName;
            test.CountOfQuestions = countOfQuestions;
            test.IsPrivate = isPrivate;
            test.IsRandom = isRandom;

            if (!string.IsNullOrEmpty(testType))
            {
                if (!Enum.TryParse<TestTypeEnum>(testType, ignoreCase: true, out var parsedType))
                {
                    throw new ArgumentException($"Invalid testType value: {testType}. Expected: QuestionsOnly, SurveyOnly or Mixed.");
                }
                test.TestType = parsedType;
            }

            if (topicId.HasValue)
            {
                var topic = await _topicRepository.GetTopicByIdAsync(topicId.Value);
                if (topic == null)
                    throw new KeyNotFoundException($"Topic with ID={topicId.Value} not found.");

                test.TopicId = topicId.Value;
                test.Topic = topic;
            }

            if (timeLimitMinutes.HasValue)
            {
                test.TimeLimitMinutes = timeLimitMinutes.Value;
            }

            await _testRepository.UpdateAsync(test);
            await _testRepository.SaveChangesAsync();

            return _mapper.Map<TestDto>(test);
        }

        public async Task DeleteTestAsync(int id)
        {
            _logger.LogInformation("DeleteTestAsync(Id={Id})", id);
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={id} not found.");

            await _testRepository.DeleteAsync(test);
            await _testRepository.SaveChangesAsync();
        }

        #endregion

        #region Adding / Removing Questions

        public async Task<TestDto> AddQuestionToTest(int testId, int questionId)
        {
            _logger.LogInformation("AddQuestionToTest(QuestionId={Q}, TestId={T})", questionId, testId);

            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={testId} not found.");

            var question = await _ctx.Questions.FindAsync(questionId);
            if (question == null)
                throw new KeyNotFoundException($"Question with ID={questionId} not found.");

            var testQuestion = new TestQuestion
            {
                TestId = testId,
                QuestionId = questionId
            };

            await _testQuestionRepository.AddAsync(testQuestion);
            await _testQuestionRepository.SaveChangesAsync();

            var updatedTest = await _testRepository.GetTestByIdAsync(testId);
            return _mapper.Map<TestDto>(updatedTest);
        }

        public async Task<TestDto> RemoveQuestionFromTest(int testId, int questionId)
        {
            _logger.LogInformation("RemoveQuestionFromTest(QuestionId={Q}, TestId={T})", questionId, testId);

            var testQuestion = await _ctx.TestQuestions
                .FirstOrDefaultAsync(tq => tq.TestId == testId && tq.QuestionId == questionId);

            if (testQuestion == null)
                throw new KeyNotFoundException($"Question ID={questionId} not found in Test ID={testId}.");

            await _testQuestionRepository.DeleteAsync(testQuestion);
            await _testQuestionRepository.SaveChangesAsync();

            var updatedTest = await _testRepository.GetTestByIdAsync(testId);
            return _mapper.Map<TestDto>(updatedTest);
        }

        public async Task<List<QuestionDto>> GetQuestionsByTestId(int testId)
        {
            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={testId} not found.");

            List<Question> questions;
            if (test.IsRandom)
            {
                questions = await _questionRepository.GetRandomQuestionsAsync(test.CountOfQuestions, test.TopicId);
            }
            else
            {
                var testQuestions = await _testQuestionRepository.GetAllByTestIdAsync(testId);
                questions = testQuestions.Select(tq => tq.Question).ToList();
            }

            return _mapper.Map<List<QuestionDto>>(questions);
        }

        public async Task<List<QuestionDto>> GetTestQuestionsAsync(int testId)
        {
            _logger.LogInformation("GetTestQuestionsAsync(TestId={T})", testId);
            var questions = await _testRepository.GetTestQuestionsAsync(testId);
            if (questions == null || questions.Count == 0)
            {
                _logger.LogWarning("No questions found for TestId={T}", testId);
                return new List<QuestionDto>();
            }

            return _mapper.Map<List<QuestionDto>>(questions);
        }

        public async Task<List<QuestionDto>> GetCandidateQuestionsAsync(int testId)
        {
            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID={testId} not found.");

            if (test.IsRandom)
                throw new InvalidOperationException($"Test {testId} is random => no manual candidate questions.");

            var topicId = test.TopicId;
            var testType = test.TestType;

            if (!topicId.HasValue)
                return new List<QuestionDto>();

            var existingIds = test.TestQuestions.Select(tq => tq.QuestionId).ToHashSet();
            var allQuestions = await _questionRepository.GetAllQuestionsAsync();
            var candidateQuestions = allQuestions.Where(q => q.TopicId == topicId.Value);
            var unassigned = candidateQuestions.Where(q => !existingIds.Contains(q.Id)).ToList();

            IEnumerable<Question> filtered;
            switch (testType)
            {
                case TestTypeEnum.QuestionsOnly:
                    filtered = unassigned.Where(q =>
                        q.QuestionType == QuestionTypeEnum.SingleChoice ||
                        q.QuestionType == QuestionTypeEnum.MultipleChoice
                    );
                    break;
                case TestTypeEnum.SurveyOnly:
                    filtered = unassigned.Where(q =>
                        q.QuestionType == QuestionTypeEnum.Survey ||
                        q.QuestionType == QuestionTypeEnum.OpenText
                    );
                    break;
                case TestTypeEnum.Mixed:
                default:
                    filtered = unassigned;
                    break;
            }

            return _mapper.Map<List<QuestionDto>>(filtered);
        }

        #endregion

        #region PDF Report Generation

        public async Task<byte[]> GenerateTestReportPdfAsync(int testId)
        {
            try
            {
                var test = await _testRepository.GetTestByIdAsync(testId);
                if (test == null)
                    throw new KeyNotFoundException($"Test with ID={testId} not found.");

                int totalAttempts = await _analyticsService.GetTotalAttemptsAsync(testId);
                double averageScore = await _analyticsService.GetAverageScoreAsync(testId);
                var distribution = await _analyticsService.GetScoreDistributionAsync(testId);
                var hardest = await _analyticsService.GetHardestQuestionsAsync(testId, 5);
                var topPlayers = await _analyticsService.GetTopPlayersAsync(testId, 5);

                return await GeneratePdfWithITextSharp(
                    test,
                    totalAttempts,
                    averageScore,
                    distribution,
                    hardest,
                    topPlayers
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating PDF for Test ID={testId}");
                throw new Exception("Error generating PDF", ex);
            }
        }

        /// <summary>
        /// Generates the PDF using iTextSharp. 
        /// This version uses only the built-in Helvetica font (no external file).
        /// </summary>
        private async Task<byte[]> GeneratePdfWithITextSharp(
            Test test,
            int totalAttempts,
            double avgScore,
            List<ScoreRangeDto> distribution,
            List<HardQuestionDto> hardest,
            List<TopPlayerDto> topPlayers)
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Use only the built-in font
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            var titleFont = new Font(baseFont, 16, Font.BOLD, BaseColor.BLACK);
            var headingFont = new Font(baseFont, 13, Font.BOLD, BaseColor.BLACK);
            var normalFont = new Font(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
            var smallFont = new Font(baseFont, 9, Font.NORMAL, BaseColor.GRAY);

            // Document metadata
            doc.AddAuthor("QuizMasterAPI");
            doc.AddTitle($"Report for Test: {test.Name}");

            // Title
            var titleParagraph = new Paragraph($"Report for Test: {test.Name}\n", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            doc.Add(titleParagraph);

            // Separator line
            doc.Add(new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator()))
            {
                SpacingAfter = 10f
            });

            // Basic info table
            var infoTable = new PdfPTable(2)
            {
                WidthPercentage = 80,
                HorizontalAlignment = Element.ALIGN_LEFT,
                SpacingBefore = 5f,
                SpacingAfter = 10f
            };
            infoTable.DefaultCell.Border = Rectangle.NO_BORDER;

            infoTable.AddCell(new PdfPCell(new Phrase("Topic:", headingFont)) { Border = Rectangle.NO_BORDER });
            infoTable.AddCell(new PdfPCell(new Phrase(test.Topic?.Name ?? "---", normalFont)) { Border = Rectangle.NO_BORDER });

            infoTable.AddCell(new PdfPCell(new Phrase("Created date:", headingFont)) { Border = Rectangle.NO_BORDER });
            infoTable.AddCell(new PdfPCell(new Phrase(test.CreatedAt.ToString("yyyy-MM-dd"), normalFont)) { Border = Rectangle.NO_BORDER });

            infoTable.AddCell(new PdfPCell(new Phrase("Total attempts:", headingFont)) { Border = Rectangle.NO_BORDER });
            infoTable.AddCell(new PdfPCell(new Phrase(totalAttempts.ToString(), normalFont)) { Border = Rectangle.NO_BORDER });

            infoTable.AddCell(new PdfPCell(new Phrase("Average score (%):", headingFont)) { Border = Rectangle.NO_BORDER });
            infoTable.AddCell(new PdfPCell(new Phrase($"{avgScore:F1}%", normalFont)) { Border = Rectangle.NO_BORDER });

            doc.Add(infoTable);

            // Score Distribution
            if (distribution.Any())
            {
                doc.Add(new Paragraph("Score Distribution (by percentage):", headingFont) { SpacingAfter = 5f });

                var distTable = new PdfPTable(2)
                {
                    WidthPercentage = 50,
                    SpacingBefore = 2f,
                    SpacingAfter = 10f
                };
                distTable.SetWidths(new float[] { 60f, 40f }); // Range / People

                var cellRange = new PdfPCell(new Phrase("Score range", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                var cellCount = new PdfPCell(new Phrase("People", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                distTable.AddCell(cellRange);
                distTable.AddCell(cellCount);

                foreach (var seg in distribution)
                {
                    var rangeCell = new PdfPCell(new Phrase(seg.RangeLabel, normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    var countCell = new PdfPCell(new Phrase(seg.Count.ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };

                    distTable.AddCell(rangeCell);
                    distTable.AddCell(countCell);
                }

                doc.Add(distTable);
            }

            // Hardest questions
            if (hardest.Any())
            {
                doc.Add(new Paragraph("Hardest Questions:", headingFont) { SpacingAfter = 5f });

                var tableHard = new PdfPTable(3)
                {
                    WidthPercentage = 90,
                    SpacingBefore = 2f,
                    SpacingAfter = 10f
                };
                tableHard.SetWidths(new float[] { 60f, 20f, 20f });

                tableHard.AddCell(new PdfPCell(new Phrase("Question", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableHard.AddCell(new PdfPCell(new Phrase("Correct, %", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableHard.AddCell(new PdfPCell(new Phrase("Attempts", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                bool alternateRow = false;
                foreach (var hq in hardest)
                {
                    var bgColor = alternateRow ? new BaseColor(240, 240, 240) : BaseColor.WHITE;
                    alternateRow = !alternateRow;

                    var c1 = new PdfPCell(new Phrase(hq.QuestionText, normalFont)) { BackgroundColor = bgColor };
                    var c2 = new PdfPCell(new Phrase($"{hq.CorrectPercentage:F1}%", normalFont))
                    {
                        BackgroundColor = bgColor,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    var c3 = new PdfPCell(new Phrase(hq.AttemptsCount.ToString(), normalFont))
                    {
                        BackgroundColor = bgColor,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };

                    // highlight if under 30%
                    if (hq.CorrectPercentage < 30)
                    {
                        c2.BackgroundColor = new BaseColor(255, 200, 200);
                    }

                    tableHard.AddCell(c1);
                    tableHard.AddCell(c2);
                    tableHard.AddCell(c3);
                }
                doc.Add(tableHard);
            }

            // Top participants
            if (topPlayers.Any())
            {
                doc.Add(new Paragraph("Top Participants:", headingFont) { SpacingAfter = 5f });

                var tableTop = new PdfPTable(3)
                {
                    WidthPercentage = 80,
                    SpacingBefore = 2f,
                    SpacingAfter = 10f
                };
                tableTop.SetWidths(new float[] { 40f, 30f, 30f });

                tableTop.AddCell(new PdfPCell(new Phrase("Participant", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableTop.AddCell(new PdfPCell(new Phrase("Score, %", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
                tableTop.AddCell(new PdfPCell(new Phrase("Time", normalFont))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });

                bool alt = false;
                foreach (var tp in topPlayers)
                {
                    var bg = alt ? new BaseColor(240, 240, 240) : BaseColor.WHITE;
                    alt = !alt;

                    var nameCell = new PdfPCell(new Phrase(tp.UserFullName, normalFont)) { BackgroundColor = bg };
                    var scoreCell = new PdfPCell(new Phrase($"{tp.ScorePercent:F1}%", normalFont))
                    {
                        BackgroundColor = bg,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    var timeCell = new PdfPCell(new Phrase(tp.TimeSpentFormatted, normalFont))
                    {
                        BackgroundColor = bg,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };

                    tableTop.AddCell(nameCell);
                    tableTop.AddCell(scoreCell);
                    tableTop.AddCell(timeCell);
                }
                doc.Add(tableTop);
            }

            // Additional info
            var additionalInfo = new Paragraph(
                "Additional notes or instructions on how to interpret the report can be placed here.\n",
                normalFont
            )
            {
                SpacingBefore = 5f,
                SpacingAfter = 10f
            };
            doc.Add(additionalInfo);

            // Finish
            doc.Close();
            writer.Close();
            return ms.ToArray();
        }

        #endregion
    }
}
