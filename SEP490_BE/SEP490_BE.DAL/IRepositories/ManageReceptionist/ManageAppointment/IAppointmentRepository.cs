using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment
{
    public interface IAppointmentRepository
    {
        #region Appointment Methods
        Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Appointment?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByReceptionistIdAsync(int receptionistId, CancellationToken cancellationToken = default);
        Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
        Task DeleteAsync(int appointmentId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, int>> GetAppointmentStatisticsAsync(CancellationToken cancellationToken = default);
        #endregion

        #region User Methods
        Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<User?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken = default);
        #endregion

        #region Patient Methods
        Task<Patient?> GetPatientByIdAsync(int patientId, CancellationToken cancellationToken = default);
        Task<Patient?> GetPatientByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        #endregion

        #region Doctor Methods
        Task<Doctor?> GetDoctorByIdAsync(int doctorId, CancellationToken cancellationToken = default);
        Task<List<Doctor>> GetAllDoctorsAsync(CancellationToken cancellationToken = default);
        #endregion

        #region Receptionist Methods
        Task<Receptionist?> GetReceptionistByIdAsync(int receptionistId, CancellationToken cancellationToken = default);
        Task<Receptionist?> GetReceptionistByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        #endregion
    }
}