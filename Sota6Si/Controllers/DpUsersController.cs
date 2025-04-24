using System;
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
    public class DpUsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpUsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpUsers
        [HttpGet]
        public async Task<IActionResult> GetDpUsers()
        {
            try
            {
                var users = await _context.DpUsers.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpUsers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpUser(int id)
        {
            try
            {
                var user = await _context.DpUsers.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpUsers
        [HttpPost]
        public async Task<IActionResult> CreateDpUser([FromBody] DpUser dpUser)
        {
            try
            {
                if (dpUser == null)
                {
                    return BadRequest("User data cannot be null");
                }

                _context.Add(dpUser);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpUser), new { id = dpUser.DpUserId }, dpUser);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpUsers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpUser(int id, [FromBody] DpUser dpUser)
        {
            try
            {
                if (id != dpUser.DpUserId)
                {
                    return BadRequest("User ID mismatch");
                }

                _context.Entry(dpUser).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpUserExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Conflict("Concurrency error occurred.");
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // DELETE: api/DpUsers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpUser(int id)
        {
            try
            {
                var dpUser = await _context.DpUsers.FindAsync(id);
                if (dpUser == null)
                {
                    return NotFound();
                }

                _context.DpUsers.Remove(dpUser);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpUserExists(int id)
        {
            return _context.DpUsers.Any(e => e.DpUserId == id);
        }
    }
}
