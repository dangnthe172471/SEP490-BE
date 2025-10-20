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

        public MedicineController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var medicines = await _medicineService.GetAllAsync(cancellationToken);
            return Ok(medicines);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var medicine = await _medicineService.GetByIdAsync(id, cancellationToken);
            if (medicine == null)
                return NotFound(new { message = $"Medicine with ID {id} not found." });

            return Ok(medicine);
        }

        [HttpGet("provider/{providerId:int}")]
        public async Task<IActionResult> GetByProviderId(int providerId, CancellationToken cancellationToken)
        {
            var medicines = await _medicineService.GetByProviderIdAsync(providerId, cancellationToken);
            return Ok(medicines);
        }

        [Authorize(Roles = "Pharmacy Provider")]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // LẤY ĐÚNG userId (CHỈ từ user_id hoặc nameidentifier)
            var userId = ResolveUserIdFromClaims(User.Claims);
            if (userId is null)
                return Unauthorized("Không thể xác định UserId từ token (thiếu/không hợp lệ claim nameidentifier).");

            // Lấy ProviderId theo UserId
            var providerId = await _medicineService.GetProviderIdByUserIdAsync(userId.Value, ct);
            if (!providerId.HasValue)
                return Conflict(new { message = $"Không tìm thấy PharmacyProvider cho UserID={userId.Value}. Hãy kiểm tra bảng PharmacyProvider và connection string." });

            try
            {
                await _medicineService.CreateAsync(dto, providerId.Value, ct);
                return Ok(new { message = "Medicine added successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        //private static int? ResolveUserIdFromClaims(IEnumerable<Claim> claims)
        //{
        //    var raw = claims.FirstOrDefault(c => c.Type == "user_id")?.Value
        //           ?? claims.FirstOrDefault(c =>
        //                  c.Type == ClaimTypes.NameIdentifier
        //               || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
        //              ?.Value;

        //    return int.TryParse(raw, out var id) ? id : (int?)null;
        //}
        private static int? ResolveUserIdFromClaims(IEnumerable<Claim> claims)
        {
            // thứ tự ưu tiên: user_id, sub, sid, uid, nameidentifier
            var raw =
                claims.FirstOrDefault(c => c.Type == "user_id")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "sub")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "sid")?.Value
                ?? claims.FirstOrDefault(c => c.Type == "uid")?.Value
                ?? claims.FirstOrDefault(c =>
                       c.Type == ClaimTypes.NameIdentifier
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                   ?.Value;

            return int.TryParse(raw, out var id) ? id : (int?)null;
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _medicineService.UpdateAsync(id, dto, cancellationToken);
                return Ok(new { message = "Medicine updated successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _medicineService.SoftDeleteAsync(id, cancellationToken);
                return Ok(new { message = "Medicine changed status successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        //[Authorize(Roles = "Pharmacy Provider")]
        //[HttpGet("mine")]
        //public async Task<IActionResult> GetMine(CancellationToken ct)
        //{
        //    var userId = ResolveUserIdFromClaims(User.Claims);
        //    if (userId is null)
        //        return Unauthorized("Không thể xác định UserId từ token (thiếu/không hợp lệ claim nameidentifier).");

        //    try
        //    {
        //        var meds = await _medicineService.GetMineAsync(userId.Value, ct);
        //        return Ok(meds);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        // Trường hợp user không có bản ghi PharmacyProvider
        //        return Conflict(new { message = ex.Message });
        //    }
        //}

        [Authorize(Roles = "Pharmacy Provider")]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10,
                CancellationToken ct = default)
        {
            var userId = ResolveUserIdFromClaims(User.Claims);
            if (userId is null)
                return Unauthorized("Không thể xác định UserId từ token (thiếu/không hợp lệ claim nameidentifier).");

            try
            {
                var result = await _medicineService.GetMinePagedAsync(userId.Value, pageNumber, pageSize, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
