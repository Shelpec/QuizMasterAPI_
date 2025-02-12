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

    // Допустим, только Admin может смотреть историю конкретного UserTest
    [Authorize(Roles = "Admin")]
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

    // Создание UserTest — авторизованный пользователь начинает прохождение теста
    [Authorize]
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

    // Редактирование UserTest (не всегда нужно), пусть Admin
    [Authorize(Roles = "Admin")]
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

    // Удаление UserTest — только Admin
    [Authorize(Roles = "Admin")]
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


    // Просмотр истории ВСЕХ пользователей — только Admin
    // Просмотр истории ВСЕХ пользователей — только Admin
    [Authorize(Roles = "Admin")]
    [HttpGet("all-full")]
    public async Task<ActionResult<PaginatedResponse<UserTestHistoryDto>>> GetAllFull(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Вход в GetAllFull (UserTests) c пагинацией: page={Page}, size={Size}", page, pageSize);
        try
        {
            var result = await _userTestService.GetAllFullPaginatedAsync(page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetAllFull()");
            throw;
        }
    }


    // Просмотр истории конкретного пользователя — если нужен общий доступ, 
    // можно сделать проверку, что email в клейме совпадает с запрошенным, или Admin
    [Authorize]
    [HttpGet("by-userEmail")]
    public async Task<ActionResult<PaginatedResponse<UserTestHistoryDto>>> GetAllByUserEmail(
    [FromQuery] string email,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Вход в GetAllByUserEmail: {Email}, page={Page}, size={Size}", email, page, pageSize);

        var currentEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && email != currentEmail)
        {
            return Forbid("Вы не можете смотреть чужую историю");
        }

        try
        {
            var result = await _userTestService.GetAllByUserEmailPaginatedAsync(email, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetAllByUserEmail({Email})", email);
            throw;
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all-by-test/{testId}")]
    public async Task<ActionResult<List<UserTestHistoryDto>>> GetAllByTestId(int testId)
    {
        _logger.LogInformation("Вход в GetAllByTestId(TestId={TestId})", testId);

        try
        {
            var list = await _userTestService.GetAllHistoryByTestId(testId);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в GetAllByTestId(TestId={TestId})", testId);
            throw;
        }
    }


}
