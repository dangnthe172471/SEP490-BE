using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.DTOs.MedicineDTO;
using System.Security.Claims;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicineController : ControllerBase
    {
        private readonly IMedicineService _medicineService;
        private const string ProviderRole = "Pharmacy Provider";

        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        private static int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(raw, out var id)) return id;
            throw new UnauthorizedAccessException("Không xác định được UserId từ token.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var medicines = await _medicineService.GetAllMedicineAsync(ct);
                return Ok(medicines);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            try
            {
                // Service sẽ validate id
                var medicine = await _medicineService.GetMedicineByIdAsync(id, ct);
                return medicine is null
                    ? NotFound(new { message = $"thuốc với ID: {id} không tồn tại." })
                    : Ok(medicine);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = GetUserIdFromClaims(User);

                var providerId = await _medicineService.GetProviderIdByUserIdAsync(userId, ct);
                if (!providerId.HasValue)
                    return Forbid();

                await _medicineService.CreateMedicineAsync(dto, providerId.Value, ct);
                return Ok(new { message = "Thêm thuốc thành công." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", detail = ex.Message });
            }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetUserIdFromClaims(User);

                await _medicineService.UpdateMineAsync(userId, id, dto, ct);
                return Ok(new { message = "Cập nhật thuốc thành công." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi.", detail = ex.Message });
            }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? sort = null,
            CancellationToken ct = default)
        {
            try
            {
                var userId = GetUserIdFromClaims(User);

                var result = await _medicineService.GetMinePagedAsync(
                    userId, pageNumber, pageSize, status, sort, ct);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi.", detail = ex.Message });
            }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpGet("excel-template")]
        public async Task<IActionResult> DownloadTemplate(CancellationToken ct)
        {
            try
            {
                var bytes = await _medicineService.GenerateExcelTemplateAsync(ct);
                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "medicine_template.xlsx"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File Excel không hợp lệ." });

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { message = "Chỉ chấp nhận file Excel (.xlsx hoặc .xls)." });

            const long maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
                return BadRequest(new { message = "File không được vượt quá 10MB." });

            try
            {
                var userId = GetUserIdFromClaims(User);

                using var stream = file.OpenReadStream();
                var result = await _medicineService.ImportFromExcelAsync(userId, stream, ct);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi import Excel.", detail = ex.Message });
            }
        }
    }
}