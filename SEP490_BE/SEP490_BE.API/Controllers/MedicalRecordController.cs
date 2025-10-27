using Microsoft.AspNetCore.Mvc;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.Models;
using System.Collections.Generic;
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

        // ✅ GET: api/medicalrecord/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecord>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var record = await _medicalRecordService.GetByIdAsync(id, cancellationToken);
            if (record == null)
            {
                return NotFound(new { message = $"Medical record with ID {id} not found." });
            }
            return Ok(record);
        }

        // 🔹 (Tuỳ chọn) POST: api/medicalrecord
        // Nếu sau này bạn muốn thêm chức năng tạo hồ sơ bệnh án mới
        /*
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] MedicalRecord model, CancellationToken cancellationToken)
        {
            await _medicalRecordService.CreateAsync(model, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = model.Id }, model);
        }
        */
    }
}
