using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _service;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(IQuestionService service, ILogger<QuestionsController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllQuestions()
        {
            _logger.LogInformation("Вход в GetAllQuestions");
            try
            {
                var questions = await _service.GetAllQuestions();
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllQuestions");
                throw;
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
        {
            _logger.LogInformation("Вход в GetQuestion(Id={Id})", id);
            try
            {
                var questionDto = await _service.GetQuestionDto(id);
                return Ok(questionDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Вопрос не найден: {Message}", ex.Message);
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetQuestion(Id={Id})", id);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
        {
            _logger.LogInformation("Вход в CreateQuestion для {Text}", dto.Text);
            try
            {
                var question = await _service.CreateQuestion(dto);
                return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CreateQuestion для {Text}", dto.Text);
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
        {
            _logger.LogInformation("Вход в UpdateQuestion(Id={Id})", id);
            try
            {
                var question = await _service.UpdateQuestion(id, dto);
                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в UpdateQuestion(Id={Id})", id);
                throw;
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            _logger.LogInformation("Вход в DeleteQuestion(Id={Id})", id);
            try
            {
                await _service.DeleteQuestion(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в DeleteQuestion(Id={Id})", id);
                throw;
            }
        }

        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetRandomQuestions([FromQuery] int count)
        {
            _logger.LogInformation("Вход в GetRandomQuestions(count={Count})", count);
            if (count <= 0)
                return BadRequest("Count must be greater than 0.");

            try
            {
                var questionDtos = await _service.GetRandomQuestions(count);
                return Ok(questionDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetRandomQuestions(count={Count})", count);
                throw;
            }
        }

        [HttpPost("check-answers")]
        public async Task<ActionResult<AnswerValidationResponseDto>> CheckAnswers([FromBody] List<AnswerValidationDto> answers)
        {
            _logger.LogInformation("Вход в CheckAnswers");
            if (!answers.Any())
            {
                return BadRequest("Answers cannot be empty.");
            }

            try
            {
                var result = await _service.CheckAnswers(answers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CheckAnswers");
                throw;
            }
        }
    }
}
