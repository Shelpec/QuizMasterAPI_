using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly ITopicService _service;
        private readonly ILogger<TopicController> _logger;

        public TopicController(ITopicService service, ILogger<TopicController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // Просмотр всех топиков — допустим, только авторизованные
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicDto>>> GetAll()
        {
            _logger.LogInformation("GetAll Topics");
            var topics = await _service.GetAllTopicsAsync();
            return Ok(topics);
        }

        // Просмотр конкретного топика
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<TopicDto>> GetById(int id)
        {
            try
            {
                var topic = await _service.GetTopicByIdAsync(id);
                return Ok(topic);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // Создание — только Admin
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<TopicDto>> Create([FromBody] CreateTopicDto dto)
        {
            var created = await _service.CreateTopicAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // Редактирование — только Admin
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<TopicDto>> Update(int id, [FromBody] UpdateTopicDto dto)
        {
            try
            {
                var updated = await _service.UpdateTopicAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }

        // Удаление — только Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteTopicAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
        }
    }
}
