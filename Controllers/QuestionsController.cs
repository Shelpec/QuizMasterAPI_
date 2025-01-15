using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllQuestions()
    {
        var questions = await _questionService.GetAllQuestions();
        return Ok(questions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var question = await _questionService.GetQuestion(id);
        if (question == null)
        {
            throw new KeyNotFoundException("Question not found.");
        }
        return Ok(question);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto questionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var question = await _questionService.CreateQuestion(questionDto);
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, question);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateQuestion(int id, [FromBody] UpdateQuestionDto questionDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var question = await _questionService.UpdateQuestion(id, questionDto);
        if (question == null)
        {
            throw new KeyNotFoundException("Question not found.");
        }
        return Ok(question);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var result = await _questionService.DeleteQuestion(id);
        if (!result)
        {
            throw new KeyNotFoundException("Question not found.");
        }
        return NoContent();
    }

    [HttpPost("check-answer/{id}")]
    public async Task<IActionResult> CheckAnswer(int id, [FromBody] int selectedAnswerId)
    {
        var isCorrect = await _questionService.CheckAnswer(id, selectedAnswerId);
        return Ok(isCorrect);
    }
}
