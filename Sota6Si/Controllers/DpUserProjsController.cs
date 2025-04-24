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
    public class DpUserProjsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpUserProjsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpUserProjs
        [HttpGet]
        public async Task<IActionResult> GetDpUserProjs()
        {
            try
            {
                var userProjs = await _context.DpUserProjs.ToListAsync();
                return Ok(userProjs);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpUserProjs/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpUserProj(int id)
        {
            try
            {
                var userProj = await _context.DpUserProjs.FindAsync(id);

                if (userProj == null)
                {
                    return NotFound();
                }

                return Ok(userProj);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpUserProjs
        [HttpPost]
        public async Task<IActionResult> CreateDpUserProj([FromBody] DpUserProj dpUserProj)
        {
            try
            {
                if (dpUserProj == null)
                {
                    return BadRequest("User project data cannot be null");
                }

                _context.Add(dpUserProj);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpUserProj), new { id = dpUserProj.DpUserProjId }, dpUserProj);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpUserProjs/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpUserProj(int id, [FromBody] DpUserProj dpUserProj)
        {
            try
            {
                if (id != dpUserProj.DpUserProjId)
                {
                    return BadRequest("User project ID mismatch");
                }

                _context.Entry(dpUserProj).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpUserProjExists(id))
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

        // DELETE: api/DpUserProjs/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpUserProj(int id)
        {
            try
            {
                var dpUserProj = await _context.DpUserProjs.FindAsync(id);
                if (dpUserProj == null)
                {
                    return NotFound();
                }

                _context.DpUserProjs.Remove(dpUserProj);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpUserProjExists(int id)
        {
            return _context.DpUserProjs.Any(e => e.DpUserProjId == id);
        }
    }
}
