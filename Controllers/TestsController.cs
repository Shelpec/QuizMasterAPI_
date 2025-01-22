using Microsoft.AspNetCore.Mvc;
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

        public TestsController(ITestService testService)
        {
            _testService = testService;
        }

        /// <summary>
        /// Создаёт новый тест с указанным кол-вом вопросов.
        /// GET /api/tests/create?count=3
        /// </summary>
        [HttpGet("create")]
        public async Task<ActionResult<TestDto>> CreateTest([FromQuery] int count)
        {
            if (count <= 0)
                return BadRequest("Количество вопросов (count) должно быть больше 0.");

            // 1. Создаём тест (сущность EF).
            var test = await _testService.CreateTestAsync(count);

            // 2. Конвертируем Test -> TestDto
            var testDto = ConvertToDto(test);

            return Ok(testDto);
        }

        /// <summary>
        /// Получаем тест по Id
        /// GET /api/tests/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestDto>> GetTest(int id)
        {
            var test = await _testService.GetTestByIdAsync(id);
            if (test == null)
                return NotFound($"Тест с Id={id} не найден.");

            // Маппинг: Test -> TestDto
            return Ok(ConvertToDto(test));
        }

        /// <summary>
        /// Пример получения всех тестов (для отладки).
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestDto>>> GetAllTests()
        {
            var tests = await _testService.GetAllTestsAsync();
            // Маппим сразу все.
            var result = tests.Select(t => ConvertToDto(t)).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Маппинг сущности EF -> DTO
        /// </summary>
        private TestDto ConvertToDto(Test test)
        {
            return new TestDto
            {
                Id = test.Id,
                CreatedAt = test.CreatedAt,
                Questions = test.TestQuestions.Select(tq => new TestQuestionDto
                {
                    Id = tq.Id,
                    Question = new QuestionDto
                    {
                        Id = tq.Question.Id,
                        Text = tq.Question.Text,
                        AnswerOptions = tq.Question.AnswerOptions
                            .Select(ao => new AnswerOptionDto
                            {
                                Id = ao.Id,
                                Text = ao.Text
                                // Если хотим показать пользователю, какой ответ правильный:
                                //IsCorrect = ao.IsCorrect
                            })
                            .ToList()
                    }
                })
                .ToList()
            };
        }


        /// <summary>
        /// Проверяем ответы на тест.
        /// Пример вызова: POST /api/tests/1/check
        /// </summary>
        [HttpPost("{testId}/check")]
        public async Task<ActionResult<TestCheckResultDto>> CheckTestAnswers(int testId, [FromBody] TestCheckDto dto)
        {
            if (dto.Answers == null || !dto.Answers.Any())
            {
                return BadRequest("Список ответов пуст.");
            }

            try
            {
                var result = await _testService.CheckTestAnswersAsync(testId, dto.Answers);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
