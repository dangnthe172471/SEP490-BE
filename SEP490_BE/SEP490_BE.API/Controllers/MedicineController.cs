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
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
            => Ok(await _medicineService.GetAllMedicineAsync(cancellationToken));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var medicine = await _medicineService.GetMedicineByIdAsync(id, cancellationToken);
            return medicine is null
                ? NotFound(new { message = $"Medicine with ID {id} not found." })
                : Ok(medicine);
        }

        [HttpGet("provider/{providerId:int}")]
        public async Task<IActionResult> GetByProviderId(int providerId, CancellationToken cancellationToken)
            => Ok(await _medicineService.GetByProviderIdAsync(providerId, cancellationToken));

        [Authorize(Roles = ProviderRole)]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            int userId;
            try { userId = RequireUserId(User); }
            catch (UnauthorizedAccessException) { return Unauthorized("Thiếu/không hợp lệ claim NameIdentifier."); }

            try
            {
                var providerId = await _medicineService.GetProviderIdByUserIdAsync(userId, ct);
                if (!providerId.HasValue) return Forbid();

                if (string.IsNullOrWhiteSpace(dto.Status)) dto.Status = "Providing";
                if (dto.Status.Equals("Providing", StringComparison.OrdinalIgnoreCase)) dto.Status = "Providing";

                await _medicineService.CreateMedicineAsync(dto, providerId.Value, ct);
                return Ok(new { message = "Medicine added successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(dto.Status))
                {
                    if (dto.Status.Equals("", StringComparison.OrdinalIgnoreCase)) dto.Status = "Providing";
                }

                await _medicineService.UpdateMedicineAsync(id, dto, cancellationToken);
                return Ok(new { message = "Medicine updated successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _medicineService.SoftDeleteAsync(id, cancellationToken);
                return Ok(new { message = "Medicine changed status successfully." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
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
            catch (UnauthorizedAccessException) { return Unauthorized("Thiếu/không hợp lệ claim NameIdentifier."); }

            try
            {
                var result = await _medicineService.GetMinePagedAsync(userId, pageNumber, pageSize, status, sort, ct);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("không phải là nhà cung cấp", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }
    }
}
