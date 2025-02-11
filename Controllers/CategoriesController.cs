using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizMasterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly QuizDbContext _db;

        public CategoriesController(QuizDbContext db)
        {
            _db = db;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
        {
            var categories = await _db.Categories.ToListAsync();

            var result = new List<CategoryDto>();
            foreach (var cat in categories)
            {
                result.Add(new CategoryDto
                {
                    Id = cat.Id,
                    Name = cat.Name
                    // Description = cat.Description,
                });
            }
            return Ok(result);
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null)
                return NotFound();

            var dto = new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name
            };
            return Ok(dto);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryDto dto)
        {
            var entity = new Category
            {
                Name = dto.Name
            };

            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();

            // Заполняем dto.Id из созданной сущности
            dto.Id = entity.Id;

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null)
                return NotFound();

            // Обновляем поля
            cat.Name = dto.Name;
            // cat.Description = dto.Description;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null)
                return NotFound();

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
