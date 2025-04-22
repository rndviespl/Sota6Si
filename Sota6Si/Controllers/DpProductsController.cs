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
    public class DpProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpProducts
        [HttpGet]
        public async Task<IActionResult> GetDpProducts()
        {
            try
            {
                var products = await _context.DpProducts.Include(d => d.DpCategory).ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpProducts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpProduct(int id)
        {
            try
            {
                var product = await _context.DpProducts
                    .Include(d => d.DpCategory)
                    .FirstOrDefaultAsync(m => m.DpProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpProducts
        [HttpPost]
        public async Task<IActionResult> CreateDpProduct([FromBody] DpProduct dpProduct)
        {
            try
            {
                if (dpProduct == null)
                {
                    return BadRequest("Product data cannot be null");
                }

                _context.Add(dpProduct);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpProduct), new { id = dpProduct.DpProductId }, dpProduct);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpProducts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpProduct(int id, [FromBody] DpProduct dpProduct)
        {
            try
            {
                if (id != dpProduct.DpProductId)
                {
                    return BadRequest("Product ID mismatch");
                }

                _context.Entry(dpProduct).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpProductExists(id))
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

        // DELETE: api/DpProducts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpProduct(int id)
        {
            try
            {
                var dpProduct = await _context.DpProducts.FindAsync(id);
                if (dpProduct == null)
                {
                    return NotFound();
                }

                _context.DpProducts.Remove(dpProduct);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpProductExists(int id)
        {
            return _context.DpProducts.Any(e => e.DpProductId == id);
        }
    }
}
