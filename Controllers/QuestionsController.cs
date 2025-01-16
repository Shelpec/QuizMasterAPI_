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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestion(int id)
        {
            try
            {
                var question = await _service.GetQuestion(id);
                return Ok(question);
            }
            catch (Exception ex)
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
            {
                return BadRequest("Count must be greater than zero.");
            }

            var questions = await _service.GetRandomQuestions(count);
            return Ok(questions);
        }

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

        //[HttpGet("random-questions")]
        //public async Task<ActionResult<IEnumerable<RandomQuestionDto>>> GetRandomQuestions([FromQuery] int questionCount)
        //{
        //    if (questionCount <= 0)
        //    {
        //        return BadRequest("Question count must be greater than 0.");
        //    }

        //    var questions = await _service.GetRandomQuestionsAsync(questionCount);
        //    if (!questions.Any())
        //    {
        //        return NotFound("No questions available.");
        //    }

        //    return Ok(questions);
        //}


        //[HttpPost("check-answers")]
        //public async Task<ActionResult> CheckAnswers([FromBody] IEnumerable<AnswerSubmissionDto> answers)
        //{
        //    if (answers == null || !answers.Any())
        //    {
        //        return BadRequest("Answers cannot be empty.");
        //    }

        //    var results = await _service.CheckAnswersAsync(answers);
        //    var correctAnswersCount = results.Count(r => r.IsCorrect);

        //    return Ok(new
        //    {
        //        CorrectAnswersCount = correctAnswersCount,
        //        Results = results
        //    });
        //}

    }
}
