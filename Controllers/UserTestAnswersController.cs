using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserTestAnswersController : ControllerBase
{
    private readonly IUserTestAnswerService _answerService;
    private readonly ILogger<UserTestAnswersController> _logger;

    public UserTestAnswersController(IUserTestAnswerService answerService, ILogger<UserTestAnswersController> logger)
    {
        _answerService = answerService;
        _logger = logger;
    }

    [HttpPost("{userTestId}/save")]
    public async Task<IActionResult> SaveAnswers(int userTestId, [FromBody] List<UserAnswerSubmitDto> answers)
    {
        _logger.LogInformation("SaveAnswers(UserTestId={Id}), Count={Count}", userTestId, answers?.Count);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            await _answerService.SaveAnswersAsync(userTestId, answers, userId);
            return Ok(new { message = "Answers saved!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в SaveAnswers(UserTestId={Id})", userTestId);
            throw;
        }
    }

    [HttpGet("{userTestId}/check")]
    public async Task<ActionResult<TestCheckResultDto>> CheckAnswers(int userTestId)
    {
        _logger.LogInformation("CheckAnswers(UserTestId={Id})", userTestId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var result = await _answerService.CheckAnswersAsync(userTestId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в CheckAnswers(UserTestId={Id})", userTestId);
            throw;
        }
    }
}
