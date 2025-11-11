using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.BLL.IServices.ManageReceptionist.ManageAppointment
{
    public interface IAppointmentService
    {
        #region Appointment Methods
        Task<List<AppointmentDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<AppointmentDto?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<List<AppointmentDto>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<List<AppointmentDto>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);
        Task<List<AppointmentDto>> GetByReceptionistIdAsync(int receptionistId, CancellationToken cancellationToken = default);
        Task<int> CreateAppointmentByPatientAsync(BookAppointmentRequest request, int userId, CancellationToken cancellationToken = default);
        Task<int> CreateAppointmentByReceptionistAsync(CreateAppointmentByReceptionistRequest request, int receptionistId, CancellationToken cancellationToken = default);
        Task<bool> RescheduleAppointmentAsync(int appointmentId, int userId, RescheduleAppointmentRequest request, CancellationToken cancellationToken = default);
        Task<bool> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusRequest request, CancellationToken cancellationToken = default);
        Task<bool> CanCancelAppointmentAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<AppointmentConfirmationDto?> GetAppointmentConfirmationAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<AppointmentStatisticsDto> GetAppointmentStatisticsAsync(CancellationToken cancellationToken = default);
        Task<List<AppointmentTimeSeriesPointDto>> GetAppointmentTimeSeriesAsync(DateTime? from, DateTime? to, string groupBy, CancellationToken cancellationToken = default);
        Task<List<AppointmentHeatmapPointDto>> GetAppointmentHeatmapAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int appointmentId, CancellationToken cancellationToken = default);
        #endregion

        #region Doctor Methods
        Task<List<DoctorInfoDto>> GetAllDoctorsAsync(CancellationToken cancellationToken = default);
        Task<DoctorInfoDto?> GetDoctorByIdAsync(int doctorId, CancellationToken cancellationToken = default);
        #endregion

        #region Patient Methods
        Task<PatientInfoDto?> GetPatientByIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<PatientInfoDto?> GetPatientInfoByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        #endregion

        #region Receptionist Methods
        Task<ReceptionistInfoDto?> GetReceptionistByIdAsync(int receptionistId, CancellationToken cancellationToken = default);
        Task<ReceptionistInfoDto?> GetReceptionistByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        #endregion

        #region Debug Methods
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Patient?> GetPatientEntityByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        #endregion
    }
}