using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;
using Sota6Si.Data;
using Sota6Si.Models;
using static System.Net.Mime.MediaTypeNames;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Caching.Memory;

namespace Sota6Si.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DpImagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DpImagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DpImages
        [HttpGet]
        public async Task<IActionResult> GetDpImages()
        {
            try
            {
                var images = await _context.DpImages
                    .Select(img => new
                    {
                        img.DpImagesId,
                        img.DpProductId,
                        img.DpImageTitle,
                        // Избегаем загрузки ImagesData, если не нужно
                        ImagesData = img.ImagesData != null ? img.ImagesData.Length : 0
                    })
                    .ToListAsync();
                return Ok(images);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetDpImageData(int id, [FromServices] IMemoryCache cache)
        {
            try
            {
                var cacheKey = $"image_{id}_480";
                if (!cache.TryGetValue(cacheKey, out byte[]? cachedImage))
                {
                    var dpImage = await _context.DpImages.FindAsync(id);
                    if (dpImage == null || dpImage.ImagesData == null)
                    {
                        return NotFound();
                    }

                    using var stream = new MemoryStream(dpImage.ImagesData);
                    using var image = await SixLabors.ImageSharp.Image.LoadAsync(stream);
                    using var outputStream = new MemoryStream();
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(480, 480),
                        Mode = ResizeMode.Max
                    }));
                    await image.SaveAsync(outputStream, new JpegEncoder { Quality = 75 });

                    cachedImage = outputStream.ToArray();
                    cache.Set(cacheKey, cachedImage, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                    });
                }

                Response.Headers.Add("Cache-Control", "public, max-age=31536000");
                return File(cachedImage, "image/jpeg");
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpImages/ByProduct/{productId}
        [HttpGet("ByProduct/{productId}")]
        public async Task<IActionResult> GetImagesByProductId(int productId)
        {
            try
            {
                var images = await _context.DpImages
                    .Where(img => img.DpProductId == productId)
                    .ToListAsync();

                return Ok(images);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // GET: api/DpImages/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDpImage(int id)
        {
            try
            {
                var image = await _context.DpImages.FindAsync(id);

                if (image == null)
                {
                    return NotFound();
                }

                return Ok(image);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }
        // POST: api/DpImages
        [HttpPost]
        public async Task<IActionResult> CreateDpImage([FromForm] CreateDpImageRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest("File is not selected or has no content.");
                }

                using var memoryStream = new MemoryStream();
                await request.File.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                var dpImage = new DpImage
                {
                    DpProductId = request.DpProductId,
                    DpImageTitle = request.DpImageTitle,
                    ImagesData = imageData
                };

                _context.DpImages.Add(dpImage);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDpImage), new { id = dpImage.DpImagesId }, dpImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/DpImages/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDpImage(int id, [FromForm] UpdateDpImageRequest request)
        {
            try
            {
                var dpImage = await _context.DpImages.FindAsync(id);
                if (dpImage == null)
                {
                    return NotFound();
                }

                if (request.File != null && request.File.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await request.File.CopyToAsync(memoryStream);
                    dpImage.ImagesData = memoryStream.ToArray();
                }

                dpImage.DpProductId = request.DpProductId;
                dpImage.DpImageTitle = request.DpImageTitle;

                _context.Entry(dpImage).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DpImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict("Concurrency error occurred.");
                }
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        // DELETE: api/DpImages/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDpImage(int id)
        {
            try
            {
                var dpImage = await _context.DpImages.FindAsync(id);
                if (dpImage == null)
                {
                    return NotFound();
                }

                _context.DpImages.Remove(dpImage);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        private bool DpImageExists(int id)
        {
            return _context.DpImages.Any(e => e.DpImagesId == id);
        }
    }
}
