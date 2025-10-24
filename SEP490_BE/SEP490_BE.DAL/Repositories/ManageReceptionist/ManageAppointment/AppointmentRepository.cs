using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SEP490_BE.DAL.Repositories.ManageReceptionist.ManageAppointment
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DiamondHealthContext _dbContext;

        public AppointmentRepository(DiamondHealthContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Appointment Methods

        public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor.Room)
                .Include(a => a.Receptionist)
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor.Room)
                .Include(a => a.Receptionist)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor.Room)
                .Include(a => a.Receptionist)
                    .ThenInclude(r => r.User)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Receptionist)
                    .ThenInclude(r => r.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetByReceptionistIdAsync(int receptionistId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor.Room)
                .Where(a => a.ReceptionistId == receptionistId)
                .OrderByDescending(a => a.AppointmentDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            appointment.CreatedAt = DateTime.Now;
            await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            _dbContext.Appointments.Update(appointment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _dbContext.Appointments.FindAsync(new object[] { appointmentId }, cancellationToken);
            if (appointment != null)
            {
                _dbContext.Appointments.Remove(appointment);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Dictionary<string, int>> GetAppointmentStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var total = await _dbContext.Appointments.CountAsync(cancellationToken);
            var pending = await _dbContext.Appointments.CountAsync(a => a.Status == "Pending", cancellationToken);
            var confirmed = await _dbContext.Appointments.CountAsync(a => a.Status == "Confirmed", cancellationToken);
            var completed = await _dbContext.Appointments.CountAsync(a => a.Status == "Completed", cancellationToken);
            var cancelled = await _dbContext.Appointments.CountAsync(a => a.Status == "Cancelled", cancellationToken);
            var noShow = await _dbContext.Appointments.CountAsync(a => a.Status == "No-Show", cancellationToken);

            return new Dictionary<string, int>
            {
                { "Total", total },
                { "Pending", pending },
                { "Confirmed", confirmed },
                { "Completed", completed },
                { "Cancelled", cancelled },
                { "No-Show", noShow }
            };
        }

        #endregion

        #region User Methods

        public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        }

        public async Task<User?> GetUserByPhoneAsync(string phone, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);
        }

        #endregion

        #region Patient Methods

        public async Task<Patient?> GetPatientByIdAsync(int patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId, cancellationToken);
        }

        public async Task<Patient?> GetPatientByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        }

        #endregion

        #region Doctor Methods

        public async Task<Doctor?> GetDoctorByIdAsync(int doctorId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Room)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId, cancellationToken);
        }

        public async Task<List<Doctor>> GetAllDoctorsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Room)
                .AsNoTracking()
                .OrderBy(d => d.User.FullName)
                .ToListAsync(cancellationToken);
        }

        #endregion

        #region Receptionist Methods

        public async Task<Receptionist?> GetReceptionistByIdAsync(int receptionistId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Receptionists
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReceptionistId == receptionistId, cancellationToken);
        }

        #endregion
    }
}