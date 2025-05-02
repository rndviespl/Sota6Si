using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Sota6Si.Data;
using Sota6Si.Models;

namespace Sota6Si.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const string CartCookieKey = "Cart";

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.DpProducts
                    .Include(p => p.DpImages)
                    .ToListAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _context.DpProducts
                    .Include(p => p.DpImages)
                    .Include(p => p.DpCategory)
                    .Include(p => p.DpProductAttributes)
                        .ThenInclude(pa => pa.DpSizeNavigation)
                    .FirstOrDefaultAsync(p => p.DpProductId == id);

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

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] DpProduct dpProduct)
        {
            try
            {
                if (dpProduct == null)
                {
                    return BadRequest("Product data cannot be null");
                }

                _context.Add(dpProduct);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProduct), new { id = dpProduct.DpProductId }, dpProduct);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] DpProduct dpProduct)
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
                    if (!ProductExists(id))
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

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
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

        // POST: api/Products/AddToCart
        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromBody] ShopCartController.AddToCartRequest request)
        {
            try
            {
                if (request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than zero.");
                }

                var cartItems = GetCartFromCookies();
                var existingItem = cartItems.FirstOrDefault(i => i.ProductId == request.ProductId && i.SizeId == request.SizeId);

                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                }
                else
                {
                    cartItems.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity, SizeId = request.SizeId });
                }

                SaveCartToCookies(cartItems);

                return Ok(new { success = true, message = "Product added to cart." });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool ProductExists(int id)
        {
            return _context.DpProducts.Any(e => e.DpProductId == id);
        }

        private List<CartItem> GetCartFromCookies()
        {
            if (Request.Cookies.TryGetValue(CartCookieKey, out var cookieValue))
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(cookieValue) ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }

        private void SaveCartToCookies(List<CartItem> cartItems)
        {
            var cookieValue = JsonConvert.SerializeObject(cartItems);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true
            };
            Response.Cookies.Append(CartCookieKey, cookieValue, cookieOptions);
        }
    }
}
