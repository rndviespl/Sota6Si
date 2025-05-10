using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OfficeOpenXml;
using Sota6Si.Data;
using Sota6Si.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Sota6Si
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopCartController : ControllerBase
    {
        private const string CartCookieKey = "Cart";
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ShopCartController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var cartItems = GetCartFromCookies();
                var productIds = cartItems.Select(ci => ci.ProductId).ToList();

                var products = await _context.DpProducts
                    .Include(p => p.DpProductAttributes)
                        .ThenInclude(a => a.DpSizeNavigation)
                    .Where(p => productIds.Contains(p.DpProductId))
                    .ToListAsync();

                var viewModel = new CartViewModel
                {
                    CartItems = cartItems,
                    Products = products
                };

                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("UpdateCart")]
        public IActionResult UpdateCart([FromBody] UpdateCartRequest request)
        {
            try
            {
                if (request.Quantity < 1 || request.Quantity > 100)
                {
                    return BadRequest("Quantity must be between 1 and 100.");
                }

                var cartItems = GetCartFromCookies();
                var existingItem = cartItems.FirstOrDefault(i => i.ProductId == request.ProductId && i.SizeId == request.SizeId);

                if (existingItem != null)
                {
                    existingItem.Quantity = request.Quantity;
                }
                else
                {
                    cartItems.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity, SizeId = request.SizeId });
                }

                SaveCartToCookies(cartItems);
                return Ok(new { success = true, message = "Cart updated!" });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("AddToCart")]
        public IActionResult AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                if (request.Quantity < 1 || request.Quantity > 100)
                {
                    return BadRequest("Quantity must be between 1 and 100.");
                }

                var cartItems = GetCartFromCookies();
                var existingItem = cartItems.FirstOrDefault(item => item.ProductId == request.ProductId && item.SizeId == request.SizeId);

                if (existingItem != null)
                {
                    int totalQuantity = existingItem.Quantity + request.Quantity;

                    if (totalQuantity > 100)
                    {
                        return BadRequest("You cannot add more than 100 units of this product.");
                    }

                    existingItem.Quantity = totalQuantity;
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

        [HttpGet("quantity")]
        public IActionResult GetCartQuantity([FromQuery] int productId, [FromQuery] int sizeId)
        {
            try
            {
                var cartItems = GetCartFromCookies();
                var existingItem = cartItems.FirstOrDefault(item => item.ProductId == productId && item.SizeId == sizeId);
                int currentQuantity = existingItem?.Quantity ?? 0;

                return Ok(new { currentQuantity });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout([FromBody] List<CartItem> cartItems)
        {
            try
            {
                if (cartItems == null || !cartItems.Any())
                {
                    return BadRequest("Cart is empty.");
                }

                var jwtToken = Request.Cookies["Token"];
                Console.WriteLine($"Token: {jwtToken}"); // Log the token to verify it is being retrieved

                if (string.IsNullOrEmpty(jwtToken))
                {
                    return BadRequest("Token not found.");
                }

                var secretKey = _configuration["ApiSettings:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    return BadRequest("Secret key not found.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(jwtToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var usernameClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (usernameClaim == null)
                {
                    return BadRequest("Username not found in token.");
                }
                var username = usernameClaim.Value;

                var user = await _context.DpUsers.FirstOrDefaultAsync(u => u.DpUsername == username);
                if (user == null)
                {
                    return BadRequest("User not found.");
                }

                var order = new DpOrder
                {
                    DpUserId = user.DpUserId,
                    DpDateTimeOrder = DateTime.UtcNow,
                    DpTypeOrder = "website"
                };

                _context.DpOrders.Add(order);
                await _context.SaveChangesAsync();

                var orderId = order.DpOrderId;

                var orderDetails = new List<DpOrderDetail>();
                foreach (var item in cartItems)
                {
                    var productAttribute = await _context.DpProductAttributes
                        .Include(pa => pa.DpProduct)
                        .Include(pa => pa.DpSizeNavigation)
                        .FirstOrDefaultAsync(pa => pa.DpProductId == item.ProductId && pa.DpSize == item.SizeId);

                    if (productAttribute != null)
                    {
                        var orderComposition = new DpOrderComposition
                        {
                            DpOrderId = orderId,
                            DpAttributesId = productAttribute.DpAttributesId,
                            DpQuantity = (sbyte)item.Quantity,
                            DpCost = productAttribute.DpProduct.DpPrice
                        };

                        _context.DpOrderCompositions.Add(orderComposition);

                        orderDetails.Add(new DpOrderDetail
                        {
                            ProductTitle = productAttribute.DpProduct.DpTitle,
                            Quantity = item.Quantity,
                            SizeName = productAttribute.DpSizeNavigation.Size,
                            UnitPrice = productAttribute.DpProduct.DpPrice,
                            TotalPrice = productAttribute.DpProduct.DpPrice * item.Quantity
                        });
                    }
                    else
                    {
                        return BadRequest($"Product with ID {item.ProductId} and size {item.SizeId} not found.");
                    }
                }

                await _context.SaveChangesAsync();

                Response.Cookies.Delete(CartCookieKey);

                return Ok(new { orderId = order.DpOrderId, orderDetails });
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("RemoveFromCart")]
        public IActionResult RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            try
            {
                var cartItems = GetCartFromCookies();
                var itemToRemove = cartItems.FirstOrDefault(item => item.ProductId == request.ProductId && item.SizeId == request.SizeId);
                if (itemToRemove != null)
                {
                    cartItems.Remove(itemToRemove);
                }

                SaveCartToCookies(cartItems);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportToExcel([FromQuery] int orderId)
        {
            try
            {
                var orderDetails = await _context.DpOrderCompositions
                    .Include(oc => oc.DpAttributes.DpProduct)
                    .Where(oc => oc.DpOrderId == orderId)
                    .ToListAsync();

                if (orderDetails == null || !orderDetails.Any())
                {
                    return BadRequest("No data to export.");
                }

                var excelFile = CreateExcelFile(orderDetails);
                var fileName = $"Order_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                var stream = new MemoryStream();
                excelFile.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private ExcelPackage CreateExcelFile(List<DpOrderComposition> orderCompositions)
        {
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Order Details");

            worksheet.Cells[1, 1].Value = "Product";
            worksheet.Cells[1, 2].Value = "Quantity";
            worksheet.Cells[1, 3].Value = "Unit Price";
            worksheet.Cells[1, 4].Value = "Total Price";

            for (int i = 0; i < orderCompositions.Count; i++)
            {
                var item = orderCompositions[i];

                worksheet.Cells[i + 2, 1].Value = item.DpAttributes.DpProduct.DpTitle;
                worksheet.Cells[i + 2, 2].Value = item.DpQuantity;
                worksheet.Cells[i + 2, 3].Value = item.DpCost;
                worksheet.Cells[i + 2, 4].Value = item.DpCost * item.DpQuantity;
            }

            worksheet.Column(1).AutoFit();
            worksheet.Column(2).AutoFit();
            worksheet.Column(3).Style.Numberformat.Format = "0.00";
            worksheet.Column(4).Style.Numberformat.Format = "0.00";
            worksheet.Column(3).AutoFit();
            worksheet.Column(4).AutoFit();

            return package;
        }

        private List<CartItem> GetCartFromCookies()
        {
            if (Request.Cookies.TryGetValue(CartCookieKey, out var cookieValue))
            {
                var cartItems = JsonConvert.DeserializeObject<List<CartItem>>(cookieValue) ?? new List<CartItem>();
                return cartItems;
            }
            return new List<CartItem>();
        }

        private void SaveCartToCookies(List<CartItem> cartItems)
        {
            var cookieValue = JsonConvert.SerializeObject(cartItems);
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(3),
                HttpOnly = true
            };
            Response.Cookies.Append(CartCookieKey, cookieValue, cookieOptions);
        }

        public class UpdateCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int SizeId { get; set; }
        }

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int SizeId { get; set; }
        }

        public class RemoveFromCartRequest
        {
            public int ProductId { get; set; }
            public int SizeId { get; set; }
        }

        public class DpOrderDetail
        {
            public string? ProductTitle { get; set; }
            public int Quantity { get; set; }
            public int SizeId { get; set; }
            public string SizeName { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
