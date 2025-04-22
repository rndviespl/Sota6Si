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
    public class AchievementsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AchievementsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Achievements
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Achievement>>> GetAchievements()
        {
            var achievements = await _context.Achievements.ToListAsync();
            return Ok(achievements);
        }

        // GET: api/Achievements/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Achievement>> GetAchievement(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);

            if (achievement == null)
            {
                return NotFound();
            }

            return Ok(achievement);
        }

        // POST: api/Achievements
        [HttpPost]
        public async Task<ActionResult<Achievement>> CreateAchievement([FromBody] Achievement achievement)
        {
            if (achievement == null)
            {
                return BadRequest("Achievement data cannot be null");
            }

            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAchievement), new { id = achievement.AchievementId }, achievement);
        }

        // PUT: api/Achievements/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAchievement(int id, [FromBody] Achievement achievement)
        {
            if (id != achievement.AchievementId)
            {
                return BadRequest("Achievement ID mismatch");
            }

            _context.Entry(achievement).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AchievementExists(id))
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

        // DELETE: api/Achievements/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAchievement(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }

            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AchievementExists(int id)
        {
            return _context.Achievements.Any(e => e.AchievementId == id);
        }
    }
}
