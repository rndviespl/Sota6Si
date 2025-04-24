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
    public class DpOrderCompositionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpOrderCompositionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpOrderCompositions
        [HttpGet]
        public async Task<IActionResult> GetDpOrderCompositions()
        {
            try
            {
                var orderCompositions = await _context.DpOrderCompositions
                    .Include(oc => oc.DpAttributes)
                    .Include(oc => oc.DpOrder)
                    .ToListAsync();
                return Ok(orderCompositions);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpOrderCompositions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpOrderComposition(int id)
        {
            try
            {
                var orderComposition = await _context.DpOrderCompositions
                    .Include(oc => oc.DpAttributes)
                    .Include(oc => oc.DpOrder)
                    .FirstOrDefaultAsync(oc => oc.DpOrderId == id);

                if (orderComposition == null)
                {
                    return NotFound();
                }

                return Ok(orderComposition);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpOrderCompositions
        [HttpPost]
        public async Task<IActionResult> CreateDpOrderComposition([FromBody] DpOrderComposition dpOrderComposition)
        {
            try
            {
                if (dpOrderComposition == null)
                {
                    return BadRequest("Order composition data cannot be null");
                }

                _context.Add(dpOrderComposition);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpOrderComposition), new { id = dpOrderComposition.DpOrderId }, dpOrderComposition);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpOrderCompositions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpOrderComposition(int id, [FromBody] DpOrderComposition dpOrderComposition)
        {
            try
            {
                if (id != dpOrderComposition.DpOrderId)
                {
                    return BadRequest("Order composition ID mismatch");
                }

                _context.Entry(dpOrderComposition).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpOrderCompositionExists(id))
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

        // DELETE: api/DpOrderCompositions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpOrderComposition(int id)
        {
            try
            {
                var dpOrderComposition = await _context.DpOrderCompositions.FindAsync(id);
                if (dpOrderComposition == null)
                {
                    return NotFound();
                }

                _context.DpOrderCompositions.Remove(dpOrderComposition);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpOrderCompositionExists(int id)
        {
            return _context.DpOrderCompositions.Any(e => e.DpOrderId == id);
        }
    }
}
