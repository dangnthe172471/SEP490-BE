using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices
{
    public interface IMedicalRecordService
    {
        Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<MedicalRecord>> GetAllByDoctorAsync(int id, CancellationToken cancellationToken = default);
        Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<MedicalRecord> CreateAsync(CreateMedicalRecordDto dto, CancellationToken cancellationToken = default);
        Task<MedicalRecord?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken cancellationToken = default);
        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<bool> IsRecordOwnedByDoctorAsync(int recordId, int userId, CancellationToken cancellationToken = default);
        Task<bool> IsAppointmentOwnedByDoctorAsync(int appointmentId, int userId, CancellationToken cancellationToken = default);
    }
}
