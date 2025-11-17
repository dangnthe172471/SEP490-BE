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

        private static int RequireUserId(ClaimsPrincipal user)
        {
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(raw, out var id)) return id;
            throw new UnauthorizedAccessException("Không xác định được UserId từ token.");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _medicineService.GetAllMedicineAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id, ct);
            return medicine is null
                ? NotFound(new { message = $"Medicine with ID {id} not found." })
                : Ok(medicine);
        }

        [Authorize(Roles = ProviderRole)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int userId;
            try { userId = RequireUserId(User); }
            catch { return Unauthorized("Thiếu hoặc không hợp lệ claim NameIdentifier."); }

            try
            {
                var providerId = await _medicineService.GetProviderIdByUserIdAsync(userId, ct);
                if (!providerId.HasValue) return Forbid();

                await _medicineService.CreateMedicineAsync(dto, providerId.Value, ct);
                return Ok(new { message = "Medicine added successfully." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [Authorize(Roles = ProviderRole)]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto, CancellationToken ct)
        {
            int userId;
            try { userId = RequireUserId(User); }
            catch { return Unauthorized("Thiếu hoặc không hợp lệ claim NameIdentifier."); }

            try
            {
                await _medicineService.UpdateMineAsync(userId, id, dto, ct);
                return Ok(new { message = "Medicine updated successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
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
            int userId;
            try { userId = RequireUserId(User); }
            catch { return Unauthorized("Thiếu hoặc không hợp lệ claim NameIdentifier."); }

            try
            {
                var result = await _medicineService.GetMinePagedAsync(userId, pageNumber, pageSize, status, sort, ct);
                return Ok(result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (UnauthorizedAccessException) { return Forbid(); }
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

            int userId;
            try { userId = RequireUserId(User); }
            catch { return Unauthorized("Thiếu hoặc không hợp lệ claim NameIdentifier."); }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _medicineService.ImportFromExcelAsync(userId, stream, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
