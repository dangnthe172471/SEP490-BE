using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories
{
    public interface IMedicalRecordRepository
    {
        Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<MedicalRecord> CreateAsync(MedicalRecord record, CancellationToken cancellationToken = default);

        Task<MedicalRecord?> UpdateAsync(int id, string? doctorNotes, string? diagnosis, CancellationToken cancellationToken = default);

        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

    }
}
