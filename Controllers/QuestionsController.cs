using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Models;
using System.Reflection.Metadata.Ecma335;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public QuestionsController(QuizDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
        {
            return await _context.Questions.Include(q => q.AnswerOptions).ToListAsync();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions.Include(q => q.AnswerOptions).FirstOrDefaultAsync(q => q.Id == id);
            if(question == null)
            {
                return NotFound();
            }
            return question;
        }

        [HttpPost]
        public async Task<ActionResult<Question>> CreateQuestion(Question question)
        {
            foreach (var answerOption in question.AnswerOptions)
            {
                answerOption.QuestionId = question.Id;
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
        }
        //update
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuestion(int id, Question updatedQuestion)
        {
            if(id != updatedQuestion.Id)
            {
                return BadRequest("Id вопроса не совпадает.");
            }
            var existingQuestion = await _context.Questions.Include(q => q.AnswerOptions).FirstOrDefaultAsync(q => q.Id == id);
            if(existingQuestion == null)
            {
                return NotFound();
            }
            //update question
            existingQuestion.Text = updatedQuestion.Text;
            existingQuestion.CorrectAnswerId = updatedQuestion.CorrectAnswerId;
            //update answer
            _context.AnswerOptions.RemoveRange(existingQuestion.AnswerOptions);
            foreach(var answerOption in updatedQuestion.AnswerOptions)
            {
                answerOption.QuestionId = id;
                _context.AnswerOptions.Add(answerOption);
            }
            //save update
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions.Include(q => q.AnswerOptions).FirstOrDefaultAsync(q => q.Id == id);
            if(question == null)
            {
                return NotFound();
            }

            //delete question and answer
            _context.AnswerOptions.RemoveRange(question.AnswerOptions);
            _context.Questions.Remove(question);

            //save
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /////////////////////////////////////
        [HttpPost("check-answer/{id}")]
        public async Task<ActionResult<bool>> CheckAnswer(int id, [FromBody] int selectedAnswerId)
        {
            var question = await _context.Questions
                .Include(q => q.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                return NotFound("Question not found.");
            }

            bool isCorrect = question.CorrectAnswerId == selectedAnswerId;

            return Ok(isCorrect); 
        }
    }
}
