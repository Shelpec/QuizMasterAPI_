using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly ITestService _testService;
        private readonly ILogger<TestsController> _logger;

        public TestsController(ITestService testService, ILogger<TestsController> logger)
        {
            _testService = testService;
            _logger = logger;
        }

        // Создание (раньше CreateTemplate)
        // Добавим сюда isRandom и testType:
        [Authorize(Roles = "Admin")]
        [HttpPost("create-template")]
        public async Task<ActionResult<TestDto>> CreateTemplate(
            [FromQuery] string name,
            [FromQuery] int countOfQuestions,
            [FromQuery] int topicId,
            [FromQuery] bool isPrivate = false,
            [FromQuery] bool isRandom = false,
            [FromQuery] string? testType = null,
            [FromQuery] int? timeLimitMinutes = null
        )
        {
            _logger.LogInformation("CreateTemplate: {Name}, count={Count}, topic={Topic}, isPrivate={Priv}, isRandom={Rand}, testType={Ttype}",
                name, countOfQuestions, topicId, isPrivate, isRandom, testType);

            if (countOfQuestions <= 0)
                return BadRequest("Количество вопросов должно быть > 0.");

            try
            {
                var created = await _testService.CreateTemplateAsync(
                    name,
                    countOfQuestions,
                    topicId,
                    isPrivate,
                    isRandom,
                    testType,
                    timeLimitMinutes

                );
                return Ok(created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CreateTemplate({Name})", name);
                throw;
            }
        }

        // Просмотр одного теста
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TestDto>> GetTest(int id)
        {
            _logger.LogInformation("Вход в GetTest(Id={Id})", id);
            try
            {
                var test = await _testService.GetTestByIdAsync(id);
                if (test == null)
                    return NotFound($"Шаблон с Id={id} не найден.");

                return Ok(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetTest(Id={Id})", id);
                throw;
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TestDto>>> GetAllTests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Вход в GetAllTests c пагинацией: page={Page}, pageSize={Size}", page, pageSize);

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                bool isAdmin = User.IsInRole("Admin");

                var result = await _testService.GetAllTestsPaginatedAsync(page, pageSize, userId, isAdmin);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllTests");
                throw;
            }
        }

        // Обновление теста
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<TestDto>> UpdateTest(
            int id,
            [FromQuery] string newName,
            [FromQuery] int countOfQuestions,
            [FromQuery] int? topicId,
            [FromQuery] bool isPrivate = false,
            [FromQuery] bool isRandom = false,
            [FromQuery] string? testType = null,
            [FromQuery] int? timeLimitMinutes = null
        )
        {
            _logger.LogInformation("UpdateTest(Id={Id}), isPrivate={Priv}, isRandom={Rand}, testType={Ttype}", id, isPrivate, isRandom, testType);
            try
            {
                var updated = await _testService.UpdateTestAsync(id, newName, countOfQuestions, topicId, isPrivate, isRandom, testType, timeLimitMinutes);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("UpdateTest: не найден Id={Id}. {Message}", id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в UpdateTest(Id={Id})", id);
                throw;
            }
        }

        // Удаление — только Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            _logger.LogInformation("Вход в DeleteTest(Id={Id})", id);
            try
            {
                await _testService.DeleteTestAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("DeleteTest: не найден Id={Id}. {Message}", id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в DeleteTest(Id={Id})", id);
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{testId}/questions/{questionId}")]
        public async Task<ActionResult<TestDto>> AddQuestionToTest(int testId, int questionId)
        {
            _logger.LogInformation("Добавляем вопрос {QuestionId} в тест {TestId}", questionId, testId);
            try
            {
                var updatedTest = await _testService.AddQuestionToTest(testId, questionId);
                return Ok(updatedTest);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении вопроса {QuestionId} в тест {TestId}", questionId, testId);
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{testId}/questions/{questionId}")]
        public async Task<ActionResult<TestDto>> RemoveQuestionFromTest(int testId, int questionId)
        {
            _logger.LogInformation("Удаляем вопрос {QuestionId} из теста {TestId}", questionId, testId);
            try
            {
                var updatedTest = await _testService.RemoveQuestionFromTest(testId, questionId);
                return Ok(updatedTest);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении вопроса {QuestionId} из теста {TestId}", questionId, testId);
                throw;
            }
        }

        [HttpGet("{testId}/questions")]
        public async Task<ActionResult<List<QuestionDto>>> GetTestQuestions(int testId)
        {
            _logger.LogInformation("Запрос вопросов для теста {TestId}", testId);

            try
            {
                var questions = await _testService.GetTestQuestionsAsync(testId);
                if (questions.Count == 0)
                {
                    return NotFound($"Вопросы для теста {testId} не найдены.");
                }
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении вопросов теста {TestId}", testId);
                return StatusCode(500, "Ошибка сервера");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{testId}/candidate-questions")]
        public async Task<ActionResult<List<QuestionDto>>> GetCandidateQuestions(int testId)
        {
            _logger.LogInformation("Запрос кандидатных вопросов для теста {TestId}", testId);

            try
            {
                var candidateQuestions = await _testService.GetCandidateQuestionsAsync(testId);
                return Ok(candidateQuestions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении кандидатных вопросов для теста {TestId}", testId);
                return StatusCode(500, "Ошибка сервера");
            }
        }



        [HttpGet("{testId}/report/pdf")]
        public async Task<IActionResult> GetTestReportPdf(int testId)
        {
            try
            {
                byte[] pdfBytes = await _testService.GenerateTestReportPdfAsync(testId);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound("Could not generate PDF (empty).");
                }

                return File(pdfBytes, "application/pdf", $"TestReport_{testId}.pdf");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for test={TestId}", testId);
                return StatusCode(500, "Error generating PDF");
            }
        }


    }
}
