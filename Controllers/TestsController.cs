using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestsController(ITestService testService)
        {
            _testService = testService;
        }

        /// <summary>
        /// Создаём новый "шаблон" Test.
        /// </summary>
        [Authorize]
        [HttpPost("create-template")]
        public async Task<ActionResult<Test>> CreateTemplate([FromQuery] string name, [FromQuery] int countOfQuestions, [FromQuery] int? topicId)
        {
            if (countOfQuestions <= 0)
                return BadRequest("Количество вопросов должно быть > 0.");

            var test = await _testService.CreateTemplateAsync(name, countOfQuestions, topicId);
            return Ok(test);
        }

        /// <summary>
        /// Получить шаблон теста по ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Test>> GetTest(int id)
        {
            var test = await _testService.GetTestByIdAsync(id);
            if (test == null)
                return NotFound($"Шаблон с Id={id} не найден.");

            return Ok(test);
        }

        /// <summary>
        /// Получить все шаблоны.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Test>>> GetAllTests()
        {
            var tests = await _testService.GetAllTestsAsync();
            return Ok(tests);
        }

        /// <summary>
        /// Обновить шаблон (название, кол-во вопросов, тему).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Test>> UpdateTest(int id, [FromQuery] string newName, [FromQuery] int countOfQuestions, [FromQuery] int? topicId)
        {
            try
            {
                var updated = await _testService.UpdateTestAsync(id, newName, countOfQuestions, topicId);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Удалить шаблон.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            try
            {
                await _testService.DeleteTestAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
