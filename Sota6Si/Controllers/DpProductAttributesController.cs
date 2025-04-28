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
    public class DpProductAttributesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpProductAttributesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpProductAttributes
        [HttpGet]
        public async Task<IActionResult> GetDpProductAttributes()
        {
            try
            {
                var attributes = await _context.DpProductAttributes
                    .Include(d => d.DpProduct)
                    .Include(d => d.DpSizeNavigation)
                    .ToListAsync();
                return Ok(attributes);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpProductAttributes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpProductAttribute(int id)
        {
            try
            {
                var attribute = await _context.DpProductAttributes
                    .Include(d => d.DpProduct)
                    .Include(d => d.DpSizeNavigation)
                    .FirstOrDefaultAsync(m => m.DpAttributesId == id);

                if (attribute == null)
                {
                    return NotFound();
                }

                return Ok(attribute);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpProductAttributes
        [HttpPost]
        public async Task<IActionResult> CreateDpProductAttribute([FromBody] DpProductAttribute dpProductAttribute)
        {
            try
            { 
                if (dpProductAttribute == null)
                {
                    return BadRequest("Product attribute data cannot be null");
                }

                _context.Add(dpProductAttribute);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpProductAttribute), new { id = dpProductAttribute.DpAttributesId }, dpProductAttribute);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpProductAttributes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpProductAttribute(int id, [FromBody] DpProductAttribute dpProductAttribute)
        {
            try
            {
                if (id != dpProductAttribute.DpAttributesId)
                {
                    return BadRequest("Product attribute ID mismatch");
                }

                _context.Entry(dpProductAttribute).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpProductAttributeExists(id))
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

        // DELETE: api/DpProductAttributes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpProductAttribute(int id)
        {
            try
            {
                var dpProductAttribute = await _context.DpProductAttributes.FindAsync(id);
                if (dpProductAttribute == null)
                {
                    return NotFound();
                }

                _context.DpProductAttributes.Remove(dpProductAttribute);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpProductAttributeExists(int id)
        {
            return _context.DpProductAttributes.Any(e => e.DpAttributesId == id);
        }
    }
}
