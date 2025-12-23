using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        // ✅ GET: api/medicalrecord
        [HttpGet]
        public async Task<ActionResult<List<MedicalRecord>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var records = await _medicalRecordService.GetAllAsync(cancellationToken);
            return Ok(records);
        }

        [HttpGet("by-doctor/{id}")]
        public async Task<ActionResult<List<MedicalRecord>>> GetAllByDoctorAsync(int id, CancellationToken cancellationToken)
        {
            var records = await _medicalRecordService.GetAllByDoctorAsync(id, cancellationToken);

            if (records == null || !records.Any())
                return Ok("Không có hồ sơ bệnh án");
            return Ok(records);
        }

        // ✅ GET: api/medicalrecord/{id}
        [HttpGet("{id:int}", Name = "GetMedicalRecordById")]
        [Authorize]
        public async Task<ActionResult<MedicalRecord>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            // Kiểm tra nếu user là Doctor, chỉ cho phép xem hồ sơ của chính họ
            if (User.IsInRole("Doctor"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng." });
                }

                var isOwned = await _medicalRecordService.IsRecordOwnedByDoctorAsync(id, userId, cancellationToken);
                if (!isOwned)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền xem hồ sơ bệnh án này. Chỉ bác sĩ phụ trách mới có thể xem." });
                }
            }

            var record = await _medicalRecordService.GetByIdAsync(id, cancellationToken);
            if (record == null)
            {
                return NotFound(new { message = $"Medical record with ID {id} not found." });
            }
            return Ok(record);
        }

        // ✅ POST: api/medicalrecord
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<MedicalRecord>> CreateAsync([FromBody] CreateMedicalRecordDto dto, CancellationToken cancellationToken)
        {
            // Kiểm tra quyền: chỉ bác sĩ phụ trách mới được tạo hồ sơ cho lịch hẹn của họ
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "Không thể xác định người dùng." });
            }

            var isOwned = await _medicalRecordService.IsAppointmentOwnedByDoctorAsync(dto.AppointmentId, userId, cancellationToken);
            if (!isOwned)
            {
                return StatusCode(403, new { message = "Bạn không có quyền tạo hồ sơ bệnh án cho lịch hẹn này. Chỉ bác sĩ phụ trách mới có thể tạo." });
            }

            var created = await _medicalRecordService.CreateAsync(dto, cancellationToken);
            return CreatedAtRoute("GetMedicalRecordById", new { id = created.RecordId }, created);
        }

        // ✅ PUT: api/medicalrecord/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<MedicalRecord>> UpdateAsync(int id, [FromBody] UpdateMedicalRecordDto dto, CancellationToken cancellationToken)
        {
            // Kiểm tra quyền: chỉ bác sĩ phụ trách mới được sửa
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized(new { message = "Không thể xác định người dùng." });
            }

            var isOwned = await _medicalRecordService.IsRecordOwnedByDoctorAsync(id, userId, cancellationToken);
            if (!isOwned)
            {
                return StatusCode(403, new { message = "Bạn không có quyền sửa hồ sơ bệnh án này. Chỉ bác sĩ phụ trách mới có thể sửa." });
            }

            var updated = await _medicalRecordService.UpdateAsync(id, dto, cancellationToken);
            return updated is null ? NotFound(new { message = $"Medical record with ID {id} not found." }) : Ok(updated);
        }

        // ✅ GET: api/medicalrecord/by-appointment/{appointmentId}
        [HttpGet("by-appointment/{appointmentId:int}")]
        [Authorize]
        public async Task<ActionResult<MedicalRecord>> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken)
        {
            // Kiểm tra nếu user là Doctor, chỉ cho phép xem hồ sơ của lịch hẹn họ phụ trách
            if (User.IsInRole("Doctor"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Không thể xác định người dùng." });
                }

                var isOwned = await _medicalRecordService.IsAppointmentOwnedByDoctorAsync(appointmentId, userId, cancellationToken);
                if (!isOwned)
                {
                    return StatusCode(403, new { message = "Bạn không có quyền xem hồ sơ bệnh án này. Chỉ bác sĩ phụ trách mới có thể xem." });
                }
            }

            var record = await _medicalRecordService.GetByAppointmentIdAsync(appointmentId, cancellationToken);
            return record is null ? NotFound(new { message = $"No medical record for appointment {appointmentId}." }) : Ok(record);
        }
    }
}
