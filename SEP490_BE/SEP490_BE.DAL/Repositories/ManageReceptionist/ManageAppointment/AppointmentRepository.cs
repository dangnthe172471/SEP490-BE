using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.IRepositories.ManageReceptionist.ManageAppointment;
using SEP490_BE.DAL.Models;
using SEP490_BE.DAL.DTOs.ManageReceptionist.ManageAppointment;
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

        public async Task<List<AppointmentTimeSeriesPointDto>> GetAppointmentTimeSeriesAsync(DateTime? from, DateTime? to, string groupBy, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"[AppointmentRepository] GetAppointmentTimeSeriesAsync: from={from}, to={to}, groupBy={groupBy}");

                var query = _dbContext.Appointments.AsQueryable();

                if (from.HasValue)
                {
                    var f = from.Value.Date;
                    Console.WriteLine($"[AppointmentRepository] Filtering from: {f}");
                    query = query.Where(a => a.AppointmentDate >= f);
                }
                if (to.HasValue)
                {
                    var t = to.Value.Date.AddDays(1).AddTicks(-1);
                    Console.WriteLine($"[AppointmentRepository] Filtering to: {t}");
                    query = query.Where(a => a.AppointmentDate <= t);
                }

                // Check total count before grouping
                var totalCount = await query.CountAsync(cancellationToken);
                Console.WriteLine($"[AppointmentRepository] Total appointments in range: {totalCount}");

                groupBy = (groupBy ?? "day").ToLower().Trim();

                if (groupBy == "month")
                {
                    // Query data first, then format in memory
                    var groupedData = await query
                        .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToListAsync(cancellationToken);

                    // Format period string in memory (after query execution)
                    var data = groupedData.Select(g => new AppointmentTimeSeriesPointDto
                    {
                        Period = $"{g.Year:0000}-{g.Month:00}",
                        Count = g.Count
                    }).ToList();

                    return data;
                }
                else
                {
                    // default: day
                    // Query data first, then format in memory
                    // Use EntityFunctions.TruncateTime or DbFunctions.TruncateTime for SQL Server
                    var groupedData = await query
                        .GroupBy(a => new
                        {
                            Year = a.AppointmentDate.Year,
                            Month = a.AppointmentDate.Month,
                            Day = a.AppointmentDate.Day
                        })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Day = g.Key.Day,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month).ThenBy(x => x.Day)
                        .ToListAsync(cancellationToken);

                    // Format period string in memory (after query execution)
                    var data = groupedData.Select(g => new AppointmentTimeSeriesPointDto
                    {
                        Period = $"{g.Year:0000}-{g.Month:00}-{g.Day:00}",
                        Count = g.Count
                    }).ToList();

                    Console.WriteLine($"[AppointmentRepository] GetAppointmentTimeSeriesAsync result count: {data.Count}");
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentRepository] GetAppointmentTimeSeriesAsync error: {ex.Message}");
                Console.WriteLine($"[AppointmentRepository] InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"[AppointmentRepository] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<List<AppointmentHeatmapPointDto>> GetAppointmentHeatmapAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"[AppointmentRepository] GetAppointmentHeatmapAsync: from={from}, to={to}");

                var query = _dbContext.Appointments.AsQueryable();

                if (from.HasValue)
                {
                    var f = from.Value.Date;
                    Console.WriteLine($"[AppointmentRepository] Filtering from: {f}");
                    query = query.Where(a => a.AppointmentDate >= f);
                }
                if (to.HasValue)
                {
                    var t = to.Value.Date.AddDays(1).AddTicks(-1);
                    Console.WriteLine($"[AppointmentRepository] Filtering to: {t}");
                    query = query.Where(a => a.AppointmentDate <= t);
                }

                // Check total count before grouping
                var totalCount = await query.CountAsync(cancellationToken);
                Console.WriteLine($"[AppointmentRepository] Total appointments in range: {totalCount}");

                // For better performance with large datasets, we could use SQL functions
                // But EF Core doesn't support DayOfWeek translation directly, so we load to memory
                // This is acceptable for appointment data (typically hundreds to thousands of records)
                var appointments = await query
                    .Select(a => new { a.AppointmentDate })
                    .AsNoTracking() // Improve performance by not tracking entities
                    .ToListAsync(cancellationToken);

                Console.WriteLine($"[AppointmentRepository] Loaded {appointments.Count} appointments to memory");

                // Group by weekday and hour in memory
                // Note: DayOfWeek in C#: Sunday = 0, Monday = 1, ..., Saturday = 6
                // This matches the frontend expectation (WEEKDAY_LABELS = ["CN", "T2", "T3", "T4", "T5", "T6", "T7"])
                var groupedData = appointments
                    .GroupBy(a => new
                    {
                        Weekday = (int)a.AppointmentDate.DayOfWeek,
                        Hour = a.AppointmentDate.Hour
                    })
                    .Select(g => new AppointmentHeatmapPointDto
                    {
                        Weekday = g.Key.Weekday,
                        Hour = g.Key.Hour,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Weekday)
                    .ThenBy(x => x.Hour)
                    .ToList();

                Console.WriteLine($"[AppointmentRepository] GetAppointmentHeatmapAsync result count: {groupedData.Count}");
                return groupedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppointmentRepository] GetAppointmentHeatmapAsync error: {ex.Message}");
                Console.WriteLine($"[AppointmentRepository] InnerException: {ex.InnerException?.Message}");
                Console.WriteLine($"[AppointmentRepository] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> HasAppointmentOnDateAsync(int patientId, DateTime appointmentDate, CancellationToken cancellationToken = default)
        {
            var dateOnly = appointmentDate.Date;
            return await _dbContext.Appointments
                .AnyAsync(a => a.PatientId == patientId &&
                              a.AppointmentDate.Date == dateOnly &&
                              a.Status != "Cancelled", cancellationToken);
        }

        public async Task<int> CountAppointmentsInShiftAsync(DateTime appointmentDate, int shiftId, CancellationToken cancellationToken = default)
        {
            var dateOnly = appointmentDate.Date;

            // Get shift times
            var shift = await _dbContext.Shifts.FindAsync(new object[] { shiftId }, cancellationToken);
            if (shift == null)
            {
                return 0;
            }

            // Count appointments within shift time range
            var shiftStartTime = shift.StartTime.ToTimeSpan();
            var shiftEndTime = shift.EndTime.ToTimeSpan();

            return await _dbContext.Appointments
                .Where(a => a.AppointmentDate.Date == dateOnly &&
                           a.AppointmentDate.TimeOfDay >= shiftStartTime &&
                           a.AppointmentDate.TimeOfDay < shiftEndTime &&
                           a.Status != "Cancelled")
                .CountAsync(cancellationToken);
        }

        public async Task<Shift?> GetShiftByTimeAsync(TimeOnly appointmentTime, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Shifts
                .Where(s => appointmentTime >= s.StartTime && appointmentTime < s.EndTime)
                .FirstOrDefaultAsync(cancellationToken);
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

        public async Task<Doctor?> GetDoctorByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Doctors
                .Include(d => d.User)
                .Include(d => d.Room)
                .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
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

        public async Task<Receptionist?> GetReceptionistByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Receptionists
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
        }

        #endregion
    }
}