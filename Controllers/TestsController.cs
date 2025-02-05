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

        // Создание теста (шаблона) — только Admin
        [Authorize(Roles = "Admin")]
        [HttpPost("create-template")]
        public async Task<ActionResult<Test>> CreateTemplate([FromQuery] string name, [FromQuery] int countOfQuestions, [FromQuery] int? topicId)
        {
            _logger.LogInformation("Вход в CreateTemplate: {Name}, count={Count}, topic={Topic}", name, countOfQuestions, topicId);

            if (countOfQuestions <= 0)
                return BadRequest("Количество вопросов должно быть > 0.");

            try
            {
                var test = await _testService.CreateTemplateAsync(name, countOfQuestions, topicId);
                return Ok(test);
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
        public async Task<ActionResult<Test>> GetTest(int id)
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
                var result = await _testService.GetAllTestsPaginatedAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllTests");
                throw;
            }
        }


        // Редактирование — только Admin
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Test>> UpdateTest(int id, [FromQuery] string newName, [FromQuery] int countOfQuestions, [FromQuery] int? topicId)
        {
            _logger.LogInformation("Вход в UpdateTest(Id={Id})", id);
            try
            {
                var updated = await _testService.UpdateTestAsync(id, newName, countOfQuestions, topicId);
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
    }
}
