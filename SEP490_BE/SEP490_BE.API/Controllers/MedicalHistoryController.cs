using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly DiamondHealthContext _context;

        public MedicalHistoryController(DiamondHealthContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy lịch sử bệnh án của bệnh nhân theo UserId
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMedicalHistoryByUserId(int userId)
        {
            try
            {
                // Tìm PatientId từ UserId
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy bệnh nhân" });
                }

                return await GetPatientMedicalHistory(patient.PatientId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử bệnh án của bệnh nhân
        /// </summary>
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientMedicalHistory(int patientId)
        {
            try
            {
                var medicalHistory = await _context.MedicalRecords
                    .Include(mr => mr.Appointment)
                        .ThenInclude(a => a.Doctor)
                            .ThenInclude(d => d.User)
                    .Include(mr => mr.Appointment)
                        .ThenInclude(a => a.Patient)
                            .ThenInclude(p => p.User)
                    .Include(mr => mr.Prescriptions)
                        .ThenInclude(p => p.Doctor)
                            .ThenInclude(d => d.User)
                    .Include(mr => mr.Prescriptions)
                        .ThenInclude(p => p.PrescriptionDetails)
                            .ThenInclude(pd => pd.MedicineVersion)
                    .Include(mr => mr.TestResults)
                        .ThenInclude(tr => tr.Service)
                    .Where(mr => mr.Appointment.PatientId == patientId)
                    .OrderByDescending(mr => mr.Appointment.AppointmentDate)
                    .Select(mr => new MedicalHistoryDTO
                    {
                        RecordId = mr.RecordId,
                        AppointmentId = mr.AppointmentId,
                        PatientId = mr.Appointment.PatientId,
                        PatientName = mr.Appointment.Patient.User.FullName,
                        DoctorId = mr.Appointment.DoctorId,
                        DoctorName = mr.Appointment.Doctor.User.FullName,
                        DoctorSpecialty = mr.Appointment.Doctor.Specialty,
                        AppointmentDate = mr.Appointment.AppointmentDate,
                        Diagnosis = mr.Diagnosis,
                        DoctorNotes = mr.DoctorNotes,
                        Status = mr.Appointment.Status ?? "Unknown",
                        CreatedAt = mr.CreatedAt,
                        Prescriptions = mr.Prescriptions.Select(p => new PrescriptionDTO
                        {
                            PrescriptionId = p.PrescriptionId,
                            RecordId = p.RecordId,
                            DoctorId = p.DoctorId,
                            DoctorName = p.Doctor.User.FullName,
                            IssuedDate = p.IssuedDate,
                            PrescriptionDetails = p.PrescriptionDetails.Select(pd => new PrescriptionDetailDTO
                            {
                                PrescriptionDetailId = pd.PrescriptionDetailId,
                                PrescriptionId = pd.PrescriptionId,
                                MedicineId = pd.MedicineVersion.MedicineId,
                                MedicineName = pd.MedicineVersion.MedicineName,
                                Dosage = pd.Dosage,
                                Duration = pd.Duration
                            }).ToList()
                        }).ToList(),
                        TestResults = mr.TestResults.Select(tr => new TestResultDTO
                        {
                            TestResultId = tr.TestResultId,
                            RecordId = tr.RecordId,
                            TestTypeId = tr.ServiceId,
                            TestTypeName = tr.Service.ServiceName,
                            ResultValue = tr.ResultValue,
                            Unit = tr.Unit,
                            Attachment = tr.Attachment,
                            ResultDate = tr.ResultDate,
                            Notes = tr.Notes
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = medicalHistory });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }


    }
}
