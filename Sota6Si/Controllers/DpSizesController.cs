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
    public class DpSizesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpSizesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpSizes
        [HttpGet]
        public async Task<IActionResult> GetDpSizes()
        {
            try
            {
                var sizes = await _context.DpSizes.ToListAsync();
                return Ok(sizes);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpSizes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpSize(int id)
        {
            try
            {
                var size = await _context.DpSizes.FindAsync(id);

                if (size == null)
                {
                    return NotFound();
                }

                return Ok(size);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpSizes
        [HttpPost]
        public async Task<IActionResult> CreateDpSize([FromBody] DpSize dpSize)
        {
            try
            {
                if (dpSize == null)
                {
                    return BadRequest("Size data cannot be null");
                }

                _context.Add(dpSize);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpSize), new { id = dpSize.SizeId }, dpSize);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpSizes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpSize(int id, [FromBody] DpSize dpSize)
        {
            try
            {
                if (id != dpSize.SizeId)
                {
                    return BadRequest("Size ID mismatch");
                }

                _context.Entry(dpSize).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpSizeExists(id))
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

        // DELETE: api/DpSizes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpSize(int id)
        {
            try
            {
                var dpSize = await _context.DpSizes.FindAsync(id);
                if (dpSize == null)
                {
                    return NotFound();
                }

                _context.DpSizes.Remove(dpSize);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpSizeExists(int id)
        {
            return _context.DpSizes.Any(e => e.SizeId == id);
        }
    }
}
