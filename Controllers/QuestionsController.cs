using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Services;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionsController(IQuestionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuestions()
        {
            var questions = await _service.GetAllQuestions();
            return Ok(questions);
        }


        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionDto>> GetQuestion(int id)
        {
            try
            {
                var questionDto = await _service.GetQuestionDto(id);
                return Ok(questionDto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
        {
            var question = await _service.CreateQuestion(dto);
            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto dto)
        {
            var question = await _service.UpdateQuestion(id, dto);
            return Ok(question);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _service.DeleteQuestion(id);
            return NoContent();
        }

        [HttpGet("random")]
        public async Task<ActionResult<IEnumerable<QuestionDto>>> GetRandomQuestions([FromQuery] int count)
        {
            if (count <= 0)
                return BadRequest("Count must be greater than 0.");

            var questionDtos = await _service.GetRandomQuestions(count);
            return Ok(questionDtos);
        }


        //[HttpGet("random")]
        //public async Task<ActionResult<IEnumerable<QuestionDto>>> GetRandomQuestions([FromQuery] int count)
        //{
        //    if (count <= 0)
        //    {
        //        return BadRequest("Count must be greater than zero.");
        //    }

        //    var questions = await _service.GetRandomQuestions(count);
        //    return Ok(questions);
        //}

        [HttpPost("check-answers")]
        public async Task<ActionResult<AnswerValidationResponseDto>> CheckAnswers([FromBody] List<AnswerValidationDto> answers)
        {
            if (!answers.Any())
            {
                return BadRequest("Answers cannot be empty.");
            }

            var result = await _service.CheckAnswers(answers);
            return Ok(result);
        }

    }
}
