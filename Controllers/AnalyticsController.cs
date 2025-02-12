using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Только админ видит аналитику
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        [HttpGet("{testId}")]
        public async Task<ActionResult<TestAnalyticsDto>> GetTestAnalytics(int testId)
        {
            _logger.LogInformation("Запрошена аналитика для TestId={TestId}", testId);

            try
            {
                var dto = await _analyticsService.GetTestAnalyticsAsync(testId);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении аналитики для TestId={TestId}", testId);
                throw;
            }
        }

        // 2) Новый endpoint: аналитика + история
        [HttpGet("full/{testId}")]
        public async Task<ActionResult<TestAnalyticsWithHistoryDto>> GetTestAnalyticsWithHistory(int testId)
        {
            _logger.LogInformation("GetTestAnalyticsWithHistory(TestId={TestId})", testId);
            try
            {
                var dto = await _analyticsService.GetTestAnalyticsAndHistoryAsync(testId);
                return Ok(dto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении аналитики + истории для TestId={TestId}", testId);
                throw;
            }
        }
    }
}
