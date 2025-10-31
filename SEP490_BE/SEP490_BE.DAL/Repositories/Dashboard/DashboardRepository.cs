using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_BE.DAL.DTOs.Dashboard;
using SEP490_BE.DAL.IRepositories.Dashboard;
using SEP490_BE.DAL.Models;

namespace SEP490_BE.DAL.Repositories.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DiamondHealthContext _db;

        public DashboardRepository(DiamondHealthContext db)
        {
            _db = db;
        }

        public async Task<ClinicStatusDto> GetClinicStatusAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = date.ToDateTime(new TimeOnly(23, 59, 59));

            var statuses = await _db.Appointments.AsNoTracking()
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .Select(a => a.Status ?? "")
                .ToListAsync(cancellationToken);

            int total = statuses.Count;
            int pending = statuses.Count(s => s == "Pending");
            int confirmed = statuses.Count(s => s == "Confirmed");
            int completed = statuses.Count(s => s == "Completed");
            int cancelled = statuses.Count(s => s == "Cancelled");

            var firstAppointmentsToday = await _db.Appointments
                .AsNoTracking()
                .GroupBy(a => a.PatientId)
                .Select(g => g.Min(a => a.AppointmentDate))
                .Where(minDate => minDate >= start && minDate <= end)
                .CountAsync(cancellationToken);

            return new ClinicStatusDto
            {
                Date = date,
                TodayNewPatients = firstAppointmentsToday,
                Appointments = new ClinicStatusDto.AppointmentCounters
                {
                    Total = total,
                    Pending = pending,
                    Confirmed = confirmed,
                    Completed = completed,
                    Cancelled = cancelled
                }
            };
        }

        public async Task<PatientStatisticsDto> GetPatientStatisticsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.ToDateTime(new TimeOnly(23, 59, 59));

            var patients = _db.Patients.Include(p => p.User).AsNoTracking();

            int totalPatients = await patients.CountAsync(cancellationToken);

            var genders = await patients.Select(p => (p.User.Gender ?? "").Trim().ToLower())
                .ToListAsync(cancellationToken);
            int male = genders.Count(g => g == "nam" || g == "male" || g == "m");
            int female = genders.Count(g => g == "nữ" || g == "nu" || g == "female" || g == "f");
            int other = Math.Max(0, genders.Count - male - female);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var dobs = await patients.Select(p => p.User.Dob).ToListAsync(cancellationToken);
            int g0_17 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) <= 17);
            int g18_35 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 18 && GetAge(d.Value, today) <= 35);
            int g36_55 = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 36 && GetAge(d.Value, today) <= 55);
            int g56p = dobs.Count(d => d.HasValue && GetAge(d.Value, today) >= 56);

            // Monthly appointments (count all appointments per month in range), only months that have data
            var monthlyRaw = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync(cancellationToken);

            var monthly = monthlyRaw
                .Select(x => new PatientStatisticsDto.MonthlyCount
                {
                    Month = $"{x.Year:D4}-{x.Month:D2}",
                    Count = x.Count
                })
                .ToList();

            return new PatientStatisticsDto
            {
                TotalPatients = totalPatients,
                ByGender = new PatientStatisticsDto.GenderCounters
                {
                    Male = male,
                    Female = female,
                    Other = other
                },
                ByAgeGroups = new PatientStatisticsDto.AgeGroupCounters
                {
                    _0_17 = g0_17,
                    _18_35 = g18_35,
                    _36_55 = g36_55,
                    _56_Plus = g56p
                },
                MonthlyNewPatients = monthly
            };
        }

        private static int GetAge(DateOnly dob, DateOnly today)
        {
            int age = today.Year - dob.Year;
            if (today < dob.AddYears(age)) age--;
            return age;
        }
    }
}



