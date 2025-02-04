using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserTestsController : ControllerBase
{
    private readonly IUserTestService _userTestService;
    private readonly ILogger<UserTestsController> _logger;

    public UserTestsController(IUserTestService userTestService, ILogger<UserTestsController> logger)
    {
        _userTestService = userTestService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserTestHistoryDto>> GetUserTest(int id)
    {
        _logger.LogInformation("Вход в GetUserTest(Id={Id})", id);
        try
        {
            var ut = await _userTestService.GetFullUserTestAsync(id);
            if (ut == null)
                return NotFound($"UserTest with id={id} not found.");
            return Ok(ut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetUserTest(Id={Id})", id);
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<UserTest>> CreateUserTest([FromBody] UserTest model)
    {
        _logger.LogInformation("Вход в CreateUserTest");
        try
        {
            var created = await _userTestService.CreateAsync(model);
            return CreatedAtAction(nameof(GetUserTest), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в CreateUserTest");
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserTest(int id, [FromBody] UserTest model)
    {
        _logger.LogInformation("Вход в UpdateUserTest(Id={Id})", id);
        try
        {
            model.Id = id;
            await _userTestService.UpdateAsync(model);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в UpdateUserTest(Id={Id})", id);
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserTest(int id)
    {
        _logger.LogInformation("Вход в DeleteUserTest(Id={Id})", id);
        try
        {
            await _userTestService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в DeleteUserTest(Id={Id})", id);
            throw;
        }
    }

    [Authorize]
    [HttpPost("start/{testId}")]
    public async Task<ActionResult<UserTestDto>> StartTest(int testId)
    {
        _logger.LogInformation("Вход в StartTest(TestId={TestId})", testId);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("StartTest: пользователь не авторизован");
            return Unauthorized("Пользователь не авторизован.");
        }

        try
        {
            var userTestDto = await _userTestService.StartTestAsync(testId, userId);
            return Ok(userTestDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("StartTest: не найден TestId={TestId}. {Message}", testId, ex.Message);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в StartTest(TestId={TestId})", testId);
            throw;
        }
    }

    [HttpGet("all-full")]
    public async Task<ActionResult<List<UserTestHistoryDto>>> GetAllFull()
    {
        _logger.LogInformation("Вход в GetAllFull (UserTests)");
        try
        {
            var result = await _userTestService.GetAllFullAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetAllFull()");
            throw;
        }
    }

    [HttpGet("by-userEmail")]
    public async Task<ActionResult<List<UserTestHistoryDto>>> GetAllByUserEmail([FromQuery] string email)
    {
        _logger.LogInformation("Вход в GetAllByUserEmail: {Email}", email);
        try
        {
            var result = await _userTestService.GetAllByUserEmailFullAsync(email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetAllByUserEmail({Email})", email);
            throw;
        }
    }

}
