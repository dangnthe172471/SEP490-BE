using SEP490_BE.BLL.IServices;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SEP490_BE.DAL.DTOs.MedicalRecordDTO;

namespace SEP490_BE.BLL.Services
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository;

        public MedicalRecordService(IMedicalRecordRepository medicalRecordRepository)
        {
            _medicalRecordRepository = medicalRecordRepository;
        }

        public async Task<List<MedicalRecord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.GetAllAsync(cancellationToken);
        }

        public async Task<List<MedicalRecord>> GetAllByDoctorAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.GetAllByDoctorAsync(id, cancellationToken);
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<MedicalRecord> CreateAsync(CreateMedicalRecordDto dto, CancellationToken cancellationToken = default)
        {
            // Ensure one-to-one: return existing record for this appointment if present
            var existing = await _medicalRecordRepository.GetByAppointmentIdAsync(dto.AppointmentId, cancellationToken);
            if (existing is not null)
            {
                return existing;
            }

            var entity = new MedicalRecord
            {
                AppointmentId = dto.AppointmentId,
                DoctorNotes = dto.DoctorNotes,
                Diagnosis = dto.Diagnosis
            };
            return await _medicalRecordRepository.CreateAsync(entity, cancellationToken);
        }

        public async Task<MedicalRecord?> UpdateAsync(int id, UpdateMedicalRecordDto dto, CancellationToken cancellationToken = default)
        {
            return await _medicalRecordRepository.UpdateAsync(id, dto.DoctorNotes, dto.Diagnosis, cancellationToken);
        }

        public Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            return _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        }

        public Task<bool> IsRecordOwnedByDoctorAsync(int recordId, int userId, CancellationToken cancellationToken = default)
        {
            return _medicalRecordRepository.IsRecordOwnedByDoctorAsync(recordId, userId, cancellationToken);
        }

        public Task<bool> IsAppointmentOwnedByDoctorAsync(int appointmentId, int userId, CancellationToken cancellationToken = default)
        {
            return _medicalRecordRepository.IsAppointmentOwnedByDoctorAsync(appointmentId, userId, cancellationToken);
        }
    }
}
