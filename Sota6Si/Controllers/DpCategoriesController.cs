using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sota6Si.Data;
using Sota6Si.Models;

namespace Sota6Si.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DpCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DpCategory>>> GetDpCategories()
        {
            var categories = await _context.DpCategories.ToListAsync();
            return Ok(categories);
        }

        // GET: api/DpCategories/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DpCategory>> GetDpCategory(int id)
        {
            var category = await _context.DpCategories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // POST: api/DpCategories
        [HttpPost]
        public async Task<ActionResult<DpCategory>> CreateDpCategory([FromBody] DpCategory dpCategory)
        {
            if (dpCategory == null)
            {
                return BadRequest("Category data cannot be null");
            }

            _context.DpCategories.Add(dpCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDpCategory), new { id = dpCategory.DpCategoryId }, dpCategory);
        }

        // PUT: api/DpCategories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpCategory(int id, [FromBody] DpCategory dpCategory)
        {
            if (id != dpCategory.DpCategoryId)
            {
                return BadRequest("Category ID mismatch");
            }

            _context.Entry(dpCategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DpCategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/DpCategories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpCategory(int id)
        {
            var category = await _context.DpCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.DpCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DpCategoryExists(int id)
        {
            return _context.DpCategories.Any(e => e.DpCategoryId == id);
        }
    }
}
