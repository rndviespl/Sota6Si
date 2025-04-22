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
    public class DpOrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpOrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpOrders
        [HttpGet]
        public async Task<IActionResult> GetDpOrders()
        {
            try
            {
                var orders = await _context.DpOrders.Include(d => d.DpUser).ToListAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpOrders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpOrder(int id)
        {
            try
            {
                var order = await _context.DpOrders
                    .Include(d => d.DpUser)
                    .FirstOrDefaultAsync(m => m.DpOrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/DpOrders
        [HttpPost]
        public async Task<IActionResult> CreateDpOrder([FromBody] DpOrder dpOrder)
        {
            try
            {
                if (dpOrder == null)
                {
                    return BadRequest("Order data cannot be null");
                }

                _context.Add(dpOrder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpOrder), new { id = dpOrder.DpOrderId }, dpOrder);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpOrders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpOrder(int id, [FromBody] DpOrder dpOrder)
        {
            try
            {
                if (id != dpOrder.DpOrderId)
                {
                    return BadRequest("Order ID mismatch");
                }

                _context.Entry(dpOrder).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DpOrderExists(id))
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

        // DELETE: api/DpOrders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpOrder(int id)
        {
            try
            {
                var dpOrder = await _context.DpOrders.FindAsync(id);
                if (dpOrder == null)
                {
                    return NotFound();
                }

                _context.DpOrders.Remove(dpOrder);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpOrderExists(int id)
        {
            return _context.DpOrders.Any(e => e.DpOrderId == id);
        }
    }
}
