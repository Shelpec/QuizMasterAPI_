using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using System.Security.Claims;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserTestsController : ControllerBase
    {
        private readonly IUserTestService _userTestService;

        public UserTestsController(IUserTestService userTestService)
        {
            _userTestService = userTestService;
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

        /// <summary>
        /// Проверка ответов. Пользователь отправляет userTestId и список ответов.
        /// </summary>
        [Authorize]
        [HttpPost("{userTestId}/check")]
        public async Task<ActionResult<TestCheckResultDto>> CheckAnswers(int userTestId, [FromBody] List<UserAnswerSubmitDto> answers)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Пользователь не авторизован.");

            try
            {
                var result = await _userTestService.SubmitAndCheckAnswersAsync(userTestId, answers, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                // Логируйте ошибку здесь (например, с помощью ILogger)
                return StatusCode(500, ex.Message);
            }
        }
    }
}
