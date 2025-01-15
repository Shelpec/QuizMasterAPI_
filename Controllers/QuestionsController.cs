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

        [HttpPost("check-answer/{id}")]
        public async Task<IActionResult> CheckAnswer(int id, [FromBody] int selectedAnswerId)
        {
            var isCorrect = await _service.CheckAnswer(id, selectedAnswerId);
            return Ok(isCorrect);
        }

        //[HttpPost("start-quiz")]
        //public async Task<ActionResult<int>> StartQuiz([FromBody] QuizRequestDto quizRequest)
        //{
        //    if (quizRequest.SelectedAnswers.Count != quizRequest.QuestionCount)
        //    {
        //        return BadRequest("Number of answers does not match the number of questions.");
        //    }

        //    var correctAnswersCount = await _questionService.StartQuiz(quizRequest.QuestionCount, quizRequest.SelectedAnswers);

        //    return Ok(correctAnswersCount);
        //}

    }
}
