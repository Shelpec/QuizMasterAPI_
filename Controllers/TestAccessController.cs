using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // допустим, только Админ может управлять доступом
    public class TestAccessController : ControllerBase
    {
        private readonly ITestAccessService _service;
        private readonly ILogger<TestAccessController> _logger;
        private readonly UserManager<User> _userManager; // нужно, чтобы находить Email/FullName

        public TestAccessController(ITestAccessService service,
                                    ILogger<TestAccessController> logger,
                                    UserManager<User> userManager)
        {
            _service = service;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Добавить доступ к тесту (приватному) для пользователя
        /// Возвращаем JSON { message = "Access added" }
        /// </summary>
        [HttpPost("add-access")]
        public async Task<IActionResult> AddAccess([FromQuery] int testId, [FromQuery] string userId)
        {
            _logger.LogInformation("AddAccess(TestId={TestId}, UserId={UserId})", testId, userId);
            try
            {
                await _service.AddUserAccessAsync(testId, userId);

                // Возвращаем JSON-ответ, чтобы не было ошибки парсинга
                return Ok(new { message = "Access added" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в AddAccess(TestId={TestId}, UserId={UserId})", testId, userId);
                // Можно вернуть BadRequest или throw
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Удалить доступ к тесту для пользователя
        /// Возвращаем JSON { message = "Access removed" }
        /// </summary>
        [HttpDelete("remove-access")]
        public async Task<IActionResult> RemoveAccess([FromQuery] int testId, [FromQuery] string userId)
        {
            _logger.LogInformation("RemoveAccess(TestId={TestId}, UserId={UserId})", testId, userId);
            try
            {
                await _service.RemoveUserAccessAsync(testId, userId);
                return Ok(new { message = "Access removed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в RemoveAccess(TestId={TestId}, UserId={UserId})", testId, userId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Получить список пользователей (UserId, Email, FullName), у кого есть доступ к тесту
        /// </summary>
        [HttpGet("get-users")]
        public async Task<ActionResult<List<AccessUserDto>>> GetUsers([FromQuery] int testId)
        {
            try
            {
                // Здесь _service.GetAllUsersForTestAsync(testId) вернёт List<string> (userId)
                var userIds = await _service.GetAllUsersForTestAsync(testId);

                // Теперь нужно загрузить Email/FullName для каждого userId
                var results = new List<AccessUserDto>();
                foreach (var uid in userIds)
                {
                    var user = await _userManager.FindByIdAsync(uid);
                    if (user == null) continue;

                    results.Add(new AccessUserDto
                    {
                        UserId = user.Id,
                        Email = user.Email ?? "",
                        FullName = user.FullName ?? ""
                    });
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetUsers(TestId={TestId})", testId);
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO для вывода пользователя, у которого есть доступ
    /// </summary>
    public class AccessUserDto
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
    }
}
