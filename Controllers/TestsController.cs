using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly QuizDbContext _context; // <-- добавили поле

        public TestsController(ITestService testService, QuizDbContext context)
        {
            _testService = testService;
            _context = context; // <-- инжектим контекст
        }


        /// <summary>
        /// Создаёт новый тест с указанным количеством вопросов.
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<TestDto>> CreateTest([FromQuery] int count, [FromQuery] int? topicId)
        {
            if (count <= 0)
                return BadRequest("Количество вопросов (count) должно быть больше 0.");

            // Достаём userId из токена
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Нет информации о пользователе в токене.");
            }

            // Вызываем перегруженный метод CreateTestAsync
            var test = await _testService.CreateTestAsync(count, topicId, userId);

            var testDto = ConvertToDto(test);
            return CreatedAtAction(nameof(GetTest), new { id = testDto.Id }, testDto);
        }

        /// <summary>
        /// Получить тест по ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TestDto>> GetTest(int id)
        {
            var test = await _testService.GetTestByIdAsync(id);
            if (test == null)
                return NotFound($"Тест с Id={id} не найден.");

            return Ok(ConvertToDto(test));
        }

        /// <summary>
        /// Получить все тесты.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestDto>>> GetAllTests()
        {
            var tests = await _testService.GetAllTestsAsync();
            var result = tests.Select(t => ConvertToDto(t)).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Обновить существующий тест (например, изменить связанные вопросы).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TestDto>> UpdateTest(int id, [FromBody] UpdateTestDto dto)
        {
            if (dto.QuestionIds == null || !dto.QuestionIds.Any())
                return BadRequest("Необходимо указать хотя бы один вопрос для теста.");

            try
            {
                var updatedTest = await _testService.UpdateTestAsync(id, dto.QuestionIds);
                return Ok(ConvertToDto(updatedTest));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Удалить тест по ID.
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

        /// <summary>
        /// Маппинг сущности EF -> DTO.
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
                            })
                            .ToList()
                    }
                }).ToList()
            };
        }

        /// <summary>
        /// Проверяем ответы на тест.
        /// Пример вызова: POST /api/tests/1/check
        /// </summary>
        // Пример (упрощённый), у вас может быть иначе:
        [HttpPost("{testId}/check")]
        public async Task<ActionResult<TestCheckResultDto>> CheckTestAnswers(int testId, [FromBody] TestCheckDto dto)
        {
            if (dto.Answers == null || !dto.Answers.Any())
            {
                return BadRequest("Список ответов пуст.");
            }

            // Достаём userId из токена:
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Нет информации о пользователе.");
            }

            try
            {
                var result = await _testService.CheckTestAnswersAsync(testId, dto.Answers);

                // Количество правильных
                var correctCount = result.CorrectCount;
                var totalQuestions = result.TotalQuestions;

                // Обновляем запись в UserTest
                // 1) Находим нужный UserTest по (UserId, TestId)
                var userTest = await _context.UserTests
                    .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TestId == testId);
                if (userTest == null)
                {
                    // Возможно, этот тест не принадлежит текущему пользователю,
                    // или не был создан (например, не привязан).
                    return Forbid("Этот тест не привязан к данному пользователю.");
                }

                // 2) Обновляем поля
                userTest.CorrectAnswers = correctCount;
                // userTest.TotalQuestions = totalQuestions; // Обычно уже заполнено 
                userTest.IsPassed = (correctCount > 0) && (correctCount == totalQuestions);
                // Скажем, IsPassed = true, если он ответил правильно на все вопросы.

                await _context.SaveChangesAsync();

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }
}
