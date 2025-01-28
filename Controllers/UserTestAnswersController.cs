using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserTestAnswersController : ControllerBase
{
    private readonly IUserTestAnswerService _answerService;

    public UserTestAnswersController(IUserTestAnswerService answerService)
    {
        _answerService = answerService;
    }

    // Сохранение ответов (без проверки)
    [HttpPost("{userTestId}/save")]
    public async Task<IActionResult> SaveAnswers(int userTestId, [FromBody] List<UserAnswerSubmitDto> answers)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        await _answerService.SaveAnswersAsync(userTestId, answers, userId);
        return Ok("Answers saved!");
    }

    // Проверка (динамическая)
    [HttpGet("{userTestId}/check")]
    public async Task<ActionResult<TestCheckResultDto>> CheckAnswers(int userTestId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await _answerService.CheckAnswersAsync(userTestId, userId);
        return Ok(result);
    }
}
