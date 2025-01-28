using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class UserTestsController : ControllerBase
{
    private readonly IUserTestService _userTestService;

    public UserTestsController(IUserTestService userTestService)
    {
        _userTestService = userTestService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserTest>> GetUserTest(int id)
    {
        var ut = await _userTestService.GetByIdAsync(id);
        if (ut == null) return NotFound();
        return Ok(ut);
    }

    [HttpPost]
    public async Task<ActionResult<UserTest>> CreateUserTest([FromBody] UserTest model)
    {
        // Для примера базовый CRUD
        var created = await _userTestService.CreateAsync(model);
        return CreatedAtAction(nameof(GetUserTest), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUserTest(int id, [FromBody] UserTest model)
    {
        // Проверка id == model.Id?
        model.Id = id;
        await _userTestService.UpdateAsync(model);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserTest(int id)
    {
        await _userTestService.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Начать прохождение теста (шаблона) с Id = testId.
    /// Генерируем случайные вопросы пользователю и возвращаем их с вариантами ответов и текстами вопросов.
    /// </summary>
    [Authorize]
    [HttpPost("start/{testId}")]
    public async Task<ActionResult<UserTestDto>> StartTest(int testId)
    {
        // Достаем userId из токена
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Пользователь не авторизован.");

        try
        {
            var userTestDto = await _userTestService.StartTestAsync(testId, userId);
            return Ok(userTestDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            // Логируйте ошибку здесь (например, с помощью ILogger)
            return StatusCode(500, ex.Message);
        }
    }
}