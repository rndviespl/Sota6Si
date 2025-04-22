using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sota6Si.Data;
using Sota6Si.Models;

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
                var images = await _context.DpImages.ToListAsync();
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

        // GET: api/DpImages/{id}/image
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetDpImageData(int id)
        {
            try
            {
                var dpImage = await _context.DpImages.FindAsync(id);
                if (dpImage == null || dpImage.ImagesData == null)
                {
                    return NotFound();
                }

                return File(dpImage.ImagesData, "image/png");
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
