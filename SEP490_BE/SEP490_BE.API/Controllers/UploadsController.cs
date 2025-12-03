using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("attachments")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadAttachment(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File không hợp lệ.");

            var uploadsRootFolder = Path.Combine(_env.WebRootPath, "uploads", "attachments");
            if (!Directory.Exists(uploadsRootFolder))
            {
                Directory.CreateDirectory(uploadsRootFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsRootFolder, fileName);

            await using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream, ct);
            }

            var relativePath = $"/uploads/attachments/{fileName}".Replace("\\", "/");

            return Ok(new
            {
                relativePath,
                url = relativePath
            });
        }
    }
}
